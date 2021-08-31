using Kogel.Net.WebSocket.Enums;
using Kogel.Net.WebSocket.Extension;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Kogel.Net.WebSocket.Server
{
    /// <summary>
    /// 提供WebSocket服务的管理功能
    /// </summary>
    public class WebSocketServiceManager
    {
        private volatile bool _clean;
        private Dictionary<string, WebSocketServiceHost> _hosts;
        private volatile ServerState _state;
        private object _sync;
        private TimeSpan _waitTime;

        /// <summary>
        /// 
        /// </summary>
        internal WebSocketServiceManager()
        {
            _clean = true;
            _hosts = new Dictionary<string, WebSocketServiceHost>();
            _state = ServerState.Ready;
            _sync = ((System.Collections.ICollection)_hosts).SyncRoot;
            _waitTime = TimeSpan.FromSeconds(1);
        }

        /// <summary>
        /// 获取 WebSocket 服务的数量。
        /// </summary>
        public int Count
        {
            get
            {
                lock (_sync)
                    return _hosts.Count;
            }
        }

        /// <summary>
        /// 获取 WebSocket 服务的主机实例。
        /// </summary>
        public IEnumerable<WebSocketServiceHost> Hosts
        {
            get
            {
                lock (_sync)
                    return ExtensionMethod.ToList(_hosts.Values);
            }
        }

        /// <summary>
        /// 获取具有指定路径的 WebSocket 服务的主机实例。
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public WebSocketServiceHost this[string path]
        {
            get
            {
                if (path == null)
                    throw new ArgumentNullException("path");

                if (path.Length == 0)
                    throw new ArgumentException("An empty string.", "path");

                if (path[0] != '/')
                    throw new ArgumentException("Not an absolute path.", "path");

                if (path.IndexOfAny(new[] { '?', '#' }) > -1)
                {
                    var msg = "It includes either or both query and fragment components.";
                    throw new ArgumentException(msg, "path");
                }

                WebSocketServiceHost host;
                InternalTryGetServiceHost(path, out host);

                return host;
            }
        }

        /// <summary>
        /// 获取或设置一个值，该值指示是否定期清理 WebSocket 服务中的非活动会话。
        /// </summary>
        public bool KeepClean
        {
            get
            {
                return _clean;
            }

            set
            {
                string msg;
                if (!canSet(out msg))
                {
                    Console.WriteLine(msg);
                    return;
                }

                lock (_sync)
                {
                    if (!canSet(out msg))
                    {
                        Console.WriteLine(msg);
                        return;
                    }

                    foreach (var host in _hosts.Values)
                        host.KeepClean = value;

                    _clean = value;
                }
            }
        }

        /// <summary>
        /// 获取 WebSocket 服务的路径。
        /// </summary>
        public IEnumerable<string> Paths
        {
            get
            {
                lock (_sync)
                    return ExtensionMethod.ToList(_hosts.Keys);
            }
        }

        /// <summary>
        /// 获取或设置等待响应 WebSocket Ping 或 Close 的时间。
        /// </summary>
        public TimeSpan WaitTime
        {
            get
            {
                return _waitTime;
            }

            set
            {
                if (value <= TimeSpan.Zero)
                    throw new ArgumentOutOfRangeException("value", "Zero or less.");

                string msg;
                if (!canSet(out msg))
                {
                    Console.WriteLine(msg);
                    return;
                }

                lock (_sync)
                {
                    if (!canSet(out msg))
                    {
                        Console.WriteLine(msg);
                        return;
                    }

                    foreach (var host in _hosts.Values)
                        host.WaitTime = value;

                    _waitTime = value;
                }
            }
        }

        private void broadcast(Opcode opcode, byte[] data, Action completed)
        {
            var cache = new Dictionary<CompressionMethod, byte[]>();

            try
            {
                foreach (var host in Hosts)
                {
                    if (_state != ServerState.Start)
                    {
                        Console.WriteLine("The server is shutting down.");
                        break;
                    }

                    host.Sessions.Broadcast(opcode, data, cache);
                }

                if (completed != null)
                    completed();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                cache.Clear();
            }
        }

        private void broadcast(Opcode opcode, Stream stream, Action completed)
        {
            var cache = new Dictionary<CompressionMethod, Stream>();

            try
            {
                foreach (var host in Hosts)
                {
                    if (_state != ServerState.Start)
                    {
                        Console.WriteLine("The server is shutting down.");
                        break;
                    }

                    host.Sessions.Broadcast(opcode, stream, cache);
                }

                if (completed != null)
                    completed();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                foreach (var cached in cache.Values)
                    cached.Dispose();

                cache.Clear();
            }
        }

        private void broadcastAsync(Opcode opcode, byte[] data, Action completed)
        {
            ThreadPool.QueueUserWorkItem(
              state => broadcast(opcode, data, completed)
            );
        }

        private void broadcastAsync(Opcode opcode, Stream stream, Action completed)
        {
            ThreadPool.QueueUserWorkItem(
              state => broadcast(opcode, stream, completed)
            );
        }

        private Dictionary<string, Dictionary<string, bool>> broadping(
          byte[] frameAsBytes, TimeSpan timeout
        )
        {
            var ret = new Dictionary<string, Dictionary<string, bool>>();

            foreach (var host in Hosts)
            {
                if (_state != ServerState.Start)
                {
                    Console.WriteLine("The server is shutting down.");
                    break;
                }

                var res = host.Sessions.Broadping(frameAsBytes, timeout);
                ret.Add(host.Path, res);
            }

            return ret;
        }

        private bool canSet(out string message)
        {
            message = null;

            if (_state == ServerState.Start)
            {
                message = "The server has already started.";
                return false;
            }

            if (_state == ServerState.ShuttingDown)
            {
                message = "The server is shutting down.";
                return false;
            }

            return true;
        }

        internal void Add<TBehavior>(string path, Func<TBehavior> creator)
          where TBehavior : WebSocketBehavior
        {
            path = path.TrimSlashFromEnd();

            lock (_sync)
            {
                WebSocketServiceHost host;
                if (_hosts.TryGetValue(path, out host))
                    throw new ArgumentException("Already in use.", "path");

                host = new WebSocketServiceHost<TBehavior>(path, creator, null);

                if (!_clean)
                    host.KeepClean = false;

                if (_waitTime != host.WaitTime)
                    host.WaitTime = _waitTime;

                if (_state == ServerState.Start)
                    host.Start();

                _hosts.Add(path, host);
            }
        }

        internal bool InternalTryGetServiceHost(
          string path, out WebSocketServiceHost host
        )
        {
            path = path.TrimSlashFromEnd();

            lock (_sync)
                return _hosts.TryGetValue(path, out host);
        }

        internal void Start()
        {
            lock (_sync)
            {
                foreach (var host in _hosts.Values)
                    host.Start();

                _state = ServerState.Start;
            }
        }

        internal void Stop(ushort code, string reason)
        {
            lock (_sync)
            {
                _state = ServerState.ShuttingDown;

                foreach (var host in _hosts.Values)
                    host.Stop(code, reason);

                _state = ServerState.Stop;
            }
        }

        /// <summary>
        /// 添加具有指定行为、路径和委托的 WebSocket 服务。
        /// </summary>
        /// <typeparam name="TBehavior"></typeparam>
        /// <param name="path"></param>
        /// <param name="initializer"></param>
        public void AddService<TBehavior>(string path, Action<TBehavior> initializer)
          where TBehavior : WebSocketBehavior, new()
        {
            if (path == null)
                throw new ArgumentNullException("path");

            if (path.Length == 0)
                throw new ArgumentException("An empty string.", "path");

            if (path[0] != '/')
                throw new ArgumentException("Not an absolute path.", "path");

            if (path.IndexOfAny(new[] { '?', '#' }) > -1)
            {
                var msg = "It includes either or both query and fragment components.";
                throw new ArgumentException(msg, "path");
            }

            path = path.TrimSlashFromEnd();

            lock (_sync)
            {
                WebSocketServiceHost host;
                if (_hosts.TryGetValue(path, out host))
                    throw new ArgumentException("Already in use.", "path");

                host = new WebSocketServiceHost<TBehavior>(path, () => new TBehavior(), initializer);

                if (!_clean)
                    host.KeepClean = false;

                if (_waitTime != host.WaitTime)
                    host.WaitTime = _waitTime;

                if (_state == ServerState.Start)
                    host.Start();

                _hosts.Add(path, host);
            }
        }

        /// <summary>
        /// 删除由管理器管理的所有 WebSocket 服务
        /// </summary>
        public void Clear()
        {
            List<WebSocketServiceHost> hosts = null;

            lock (_sync)
            {
                hosts = ExtensionMethod.ToList(_hosts.Values);
                _hosts.Clear();
            }

            foreach (var host in hosts)
            {
                if (host.State == ServerState.Start)
                    host.Stop(1001, String.Empty);
            }
        }

        /// <summary>
        /// 删除具有指定路径的 WebSocket 服务。
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public bool RemoveService(string path)
        {
            if (path == null)
                throw new ArgumentNullException("path");

            if (path.Length == 0)
                throw new ArgumentException("An empty string.", "path");

            if (path[0] != '/')
                throw new ArgumentException("Not an absolute path.", "path");

            if (path.IndexOfAny(new[] { '?', '#' }) > -1)
            {
                var msg = "It includes either or both query and fragment components.";
                throw new ArgumentException(msg, "path");
            }

            path = path.TrimSlashFromEnd();

            WebSocketServiceHost host;
            lock (_sync)
            {
                if (!_hosts.TryGetValue(path, out host))
                    return false;

                _hosts.Remove(path);
            }

            if (host.State == ServerState.Start)
                host.Stop(1001, String.Empty);

            return true;
        }

        /// <summary>
        /// 尝试获取具有指定路径的 WebSocket 服务的主机实例
        /// </summary>
        /// <param name="path"></param>
        /// <param name="host"></param>
        /// <returns></returns>
        public bool TryGetServiceHost(string path, out WebSocketServiceHost host)
        {
            if (path == null)
                throw new ArgumentNullException("path");

            if (path.Length == 0)
                throw new ArgumentException("An empty string.", "path");

            if (path[0] != '/')
                throw new ArgumentException("Not an absolute path.", "path");

            if (path.IndexOfAny(new[] { '?', '#' }) > -1)
            {
                var msg = "It includes either or both query and fragment components.";
                throw new ArgumentException(msg, "path");
            }

            return InternalTryGetServiceHost(path, out host);
        }
    }
}
