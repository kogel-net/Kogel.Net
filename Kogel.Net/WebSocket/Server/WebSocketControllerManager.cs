using Kogel.Net.WebSocket.Enums;
using Kogel.Net.WebSocket.Extension;
using Kogel.Net.WebSocket.Extension.Net;
using Kogel.Net.WebSocket.Interfaces;
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
    /// 为WebSocket 服务中的m每个会话提供管理功能
    /// </summary>
    public class WebSocketControllerManager
    {
        private volatile bool _clean;
        private object _forSweep;
        private Dictionary<string, IControllerSession> _sessions;
        private volatile ServerState _state;
        private volatile bool _sweeping;
        private System.Timers.Timer _sweepTimer;
        private object _sync;
        private TimeSpan _waitTime;

        internal WebSocketControllerManager()
        {
            _clean = true;
            _forSweep = new object();
            _sessions = new Dictionary<string, IControllerSession>();
            _state = ServerState.Ready;
            _sync = ((System.Collections.ICollection)_sessions).SyncRoot;
            _waitTime = TimeSpan.FromSeconds(1);

            SetSweepTimer(60000);
        }

        internal ServerState State
        {
            get
            {
                return _state;
            }
        }

        /// <summary>
        /// 获取 WebSocket 服务中活动会话的 ID
        /// </summary>
        public IEnumerable<string> ActiveIDs
        {
            get
            {
                foreach (var res in Broadping(WebSocketFrame.EmptyPingBytes))
                {
                    if (res.Value)
                        yield return res.Key;
                }
            }
        }

        //获取 WebSocket 服务中的会话数
        public int Count
        {
            get
            {
                lock (_sync)
                    return _sessions.Count;
            }
        }

        /// <summary>
        /// 获取 WebSocket 服务中会话的 ID
        /// </summary>
        public IEnumerable<string> IDs
        {
            get
            {
                if (_state != ServerState.Start)
                    return Enumerable.Empty<string>();

                lock (_sync)
                {
                    if (_state != ServerState.Start)
                        return Enumerable.Empty<string>();

                    return ExtensionMethod.ToList(_sessions.Keys);
                }
            }
        }

        /// <summary>
        /// 获取 WebSocket 服务中非活动会话的 ID
        /// </summary>
        public IEnumerable<string> InactiveIDs
        {
            get
            {
                foreach (var res in Broadping(WebSocketFrame.EmptyPingBytes))
                {
                    if (!res.Value)
                        yield return res.Key;
                }
            }
        }

        /// <summary>
        /// 获取会话实例
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IControllerSession this[string id]
        {
            get
            {
                if (id == null)
                    throw new ArgumentNullException("id");

                if (id.Length == 0)
                    throw new ArgumentException("An empty string.", "id");

                IControllerSession session;
                _TryGetSession(id, out session);

                return session;
            }
        }

        /// <summary>
        /// 获取或设置一个值，该值指示是否定期清理 WebSocket 服务中的非活动会话
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
                if (!CanSet(out msg))
                {
                    Console.WriteLine(msg);
                    return;
                }

                lock (_sync)
                {
                    if (!CanSet(out msg))
                    {
                        Console.WriteLine(msg);
                        return;
                    }

                    _clean = value;
                }
            }
        }

        /// <summary>
        /// 获取 WebSocket 服务中的会话实例
        /// </summary>
        public IEnumerable<IControllerSession> Sessions
        {
            get
            {
                if (_state != ServerState.Start)
                    return Enumerable.Empty<IControllerSession>();

                lock (_sync)
                {
                    if (_state != ServerState.Start)
                        return Enumerable.Empty<IControllerSession>();

                    return ExtensionMethod.ToList(_sessions.Values);
                }
            }
        }

        /// <summary>
        /// 获取或设置等待响应 WebSocket Ping 或 Close 的时间
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
                if (!CanSet(out msg))
                {
                    Console.WriteLine(msg);
                    return;
                }

                lock (_sync)
                {
                    if (!CanSet(out msg))
                    {
                        Console.WriteLine(msg);
                        return;
                    }

                    _waitTime = value;
                }
            }
        }

        private void _Broadcast(Opcode opcode, byte[] data, Action completed)
        {
            var cache = new Dictionary<CompressionMethod, byte[]>();

            try
            {
                foreach (var session in Sessions)
                {
                    if (_state != ServerState.Start)
                    {
                        Console.WriteLine("The service is shutting down.");
                        break;
                    }

                    session.Context.WebSocket.Send(opcode, data, cache);
                }

                if (completed != null)
                    completed();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                cache.Clear();
            }
        }

        private void _Broadcast(Opcode opcode, Stream stream, Action completed)
        {
            var cache = new Dictionary<CompressionMethod, Stream>();

            try
            {
                foreach (var session in Sessions)
                {
                    if (_state != ServerState.Start)
                    {
                        Console.WriteLine("The service is shutting down.");
                        break;
                    }

                    session.Context.WebSocket.Send(opcode, stream, cache);
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

        private void BroadcastAsync(Opcode opcode, byte[] data, Action completed)
        {
            ThreadPool.QueueUserWorkItem(
              state => _Broadcast(opcode, data, completed)
            );
        }

        private void BroadcastAsync(Opcode opcode, Stream stream, Action completed)
        {
            ThreadPool.QueueUserWorkItem(
              state => _Broadcast(opcode, stream, completed)
            );
        }

        private Dictionary<string, bool> Broadping(byte[] frameAsBytes)
        {
            var ret = new Dictionary<string, bool>();

            foreach (var session in Sessions)
            {
                if (_state != ServerState.Start)
                {
                    Console.WriteLine("The service is shutting down.");
                    break;
                }

                var res = session.Context.WebSocket.Ping(frameAsBytes, _waitTime);
                ret.Add(session.Id, res);
            }

            return ret;
        }

        private bool CanSet(out string message)
        {
            message = null;

            if (_state == ServerState.Start)
            {
                message = "The service has already started.";
                return false;
            }

            if (_state == ServerState.ShuttingDown)
            {
                message = "The service is shutting down.";
                return false;
            }

            return true;
        }

        /// <summary>
        /// 创建会话唯一标识
        /// </summary>
        /// <returns></returns>
        private static string CreateID()
        {
            return Guid.NewGuid().ToString("N");
        }

        private void SetSweepTimer(double interval)
        {
            _sweepTimer = new System.Timers.Timer(interval);
            _sweepTimer.Elapsed += (sender, e) => Sweep();
        }

        private void Stop(PayloadData payloadData, bool send)
        {
            var bytes = send
                        ? WebSocketFrame.CreateCloseFrame(payloadData, false).ToArray()
                        : null;

            lock (_sync)
            {
                _state = ServerState.ShuttingDown;

                _sweepTimer.Enabled = false;
                foreach (var session in ExtensionMethod.ToList(_sessions.Values))
                    session.Context.WebSocket.Close(payloadData, bytes);

                _state = ServerState.Stop;
            }
        }

        private bool _TryGetSession(string id, out IControllerSession session)
        {
            session = null;

            if (_state != ServerState.Start)
                return false;

            lock (_sync)
            {
                if (_state != ServerState.Start)
                    return false;

                return _sessions.TryGetValue(id, out session);
            }
        }
        internal string Add(IControllerSession session, string id)
        {
            lock (_sync)
            {
                if (_state != ServerState.Start)
                    return null;

                if (!string.IsNullOrEmpty(id))
                    id = CreateID();

                _sessions.Add(id, session);

                return id;
            }
        }

        internal void Broadcast(
          Opcode opcode, byte[] data, Dictionary<CompressionMethod, byte[]> cache
        )
        {
            foreach (var session in Sessions)
            {
                if (_state != ServerState.Start)
                {
                    Console.WriteLine("The service is shutting down.");
                    break;
                }

                session.Context.WebSocket.Send(opcode, data, cache);
            }
        }

        internal void Broadcast(
          Opcode opcode, Stream stream, Dictionary<CompressionMethod, Stream> cache
        )
        {
            foreach (var session in Sessions)
            {
                if (_state != ServerState.Start)
                {
                    Console.WriteLine("The service is shutting down.");
                    break;
                }

                session.Context.WebSocket.Send(opcode, stream, cache);
            }
        }

        internal Dictionary<string, bool> Broadping(
          byte[] frameAsBytes, TimeSpan timeout
        )
        {
            var ret = new Dictionary<string, bool>();

            foreach (var session in Sessions)
            {
                if (_state != ServerState.Start)
                {
                    Console.WriteLine("The service is shutting down.");
                    break;
                }

                var res = session.Context.WebSocket.Ping(frameAsBytes, timeout);
                ret.Add(session.Id, res);
            }

            return ret;
        }

        internal bool Remove(string id)
        {
            lock (_sync)
                return _sessions.Remove(id);
        }

        internal void Start()
        {
            lock (_sync)
            {
                _sweepTimer.Enabled = _clean;
                _state = ServerState.Start;
            }
        }

        internal void Stop(ushort code, string reason)
        {
            if (code == 1005)
            { // == no status
                Stop(PayloadData.Empty, true);
                return;
            }

            Stop(new PayloadData(code, reason), !code.IsReserved());
        }

        /// <summary>
        /// 向 WebSocket 服务中的每个客户端发送数据。
        /// </summary>
        /// <param name="data"></param>
        public void Broadcast(byte[] data)
        {
            if (_state != ServerState.Start)
            {
                var msg = "The current state of the manager is not Start.";
                throw new InvalidOperationException(msg);
            }

            if (data == null)
                throw new ArgumentNullException("data");

            if (data.LongLength <= WebSocket.FragmentLength)
                _Broadcast(Opcode.Binary, data, null);
            else
                _Broadcast(Opcode.Binary, new MemoryStream(data), null);
        }

        /// <summary>
        /// 向 WebSocket 服务中的每个客户端发送数据
        /// </summary>
        /// <param name="data"></param>
        public void Broadcast(string data)
        {
            if (_state != ServerState.Start)
            {
                var msg = "The current state of the manager is not Start.";
                throw new InvalidOperationException(msg);
            }

            if (data == null)
                throw new ArgumentNullException("data");

            byte[] bytes;
            if (!data.TryGetUTF8EncodedBytes(out bytes))
            {
                var msg = "It could not be UTF-8-encoded.";
                throw new ArgumentException(msg, "data");
            }

            if (bytes.LongLength <= WebSocket.FragmentLength)
                _Broadcast(Opcode.Text, bytes, null);
            else
                _Broadcast(Opcode.Text, new MemoryStream(bytes), null);
        }

        /// <summary>
        /// 将 data 中的数据发送到 WebSocket 服务中的每个客户端。
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="length"></param>
        public void Broadcast(Stream stream, int length)
        {
            if (_state != ServerState.Start)
            {
                var msg = "The current state of the manager is not Start.";
                throw new InvalidOperationException(msg);
            }

            if (stream == null)
                throw new ArgumentNullException("stream");

            if (!stream.CanRead)
            {
                var msg = "It cannot be read.";
                throw new ArgumentException(msg, "stream");
            }

            if (length < 1)
            {
                var msg = "Less than 1.";
                throw new ArgumentException(msg, "length");
            }

            var bytes = stream.ReadBytes(length);

            var len = bytes.Length;
            if (len == 0)
            {
                var msg = "No data could be read from it.";
                throw new ArgumentException(msg, "stream");
            }

            if (len < length)
            {
                Console.WriteLine(
                  String.Format(
                    "Only {0} byte(s) of data could be read from the stream.",
                    len
                  )
                );
            }

            if (len <= WebSocket.FragmentLength)
                _Broadcast(Opcode.Binary, bytes, null);
            else
                _Broadcast(Opcode.Binary, new MemoryStream(bytes), null);
        }

        /// <summary>
        /// 将 data 中的数据异步发送到 WebSocket 服务中的每个客户端。
        /// </summary>
        /// <param name="data"></param>
        /// <param name="completed"></param>
        public void BroadcastAsync(byte[] data, Action completed)
        {
            if (_state != ServerState.Start)
            {
                var msg = "The current state of the manager is not Start.";
                throw new InvalidOperationException(msg);
            }

            if (data == null)
                throw new ArgumentNullException("data");

            if (data.LongLength <= WebSocket.FragmentLength)
                BroadcastAsync(Opcode.Binary, data, completed);
            else
                BroadcastAsync(Opcode.Binary, new MemoryStream(data), completed);
        }

        /// <summary>
        /// 将 data 中的数据异步发送到 WebSocket 服务中的每个客户端。
        /// </summary>
        /// <param name="data"></param>
        /// <param name="completed"></param>
        public void BroadcastAsync(string data, Action completed)
        {
            if (_state != ServerState.Start)
            {
                var msg = "The current state of the manager is not Start.";
                throw new InvalidOperationException(msg);
            }

            if (data == null)
                throw new ArgumentNullException("data");

            byte[] bytes;
            if (!data.TryGetUTF8EncodedBytes(out bytes))
            {
                var msg = "It could not be UTF-8-encoded.";
                throw new ArgumentException(msg, "data");
            }

            if (bytes.LongLength <= WebSocket.FragmentLength)
                BroadcastAsync(Opcode.Text, bytes, completed);
            else
                BroadcastAsync(Opcode.Text, new MemoryStream(bytes), completed);
        }

        /// <summary>
        /// 将 data 中的数据异步发送到 WebSocket 服务中的每个客户端。
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="length"></param>
        /// <param name="completed"></param>
        public void BroadcastAsync(Stream stream, int length, Action completed)
        {
            if (_state != ServerState.Start)
            {
                var msg = "The current state of the manager is not Start.";
                throw new InvalidOperationException(msg);
            }

            if (stream == null)
                throw new ArgumentNullException("stream");

            if (!stream.CanRead)
            {
                var msg = "It cannot be read.";
                throw new ArgumentException(msg, "stream");
            }

            if (length < 1)
            {
                var msg = "Less than 1.";
                throw new ArgumentException(msg, "length");
            }

            var bytes = stream.ReadBytes(length);

            var len = bytes.Length;
            if (len == 0)
            {
                var msg = "No data could be read from it.";
                throw new ArgumentException(msg, "stream");
            }

            if (len < length)
            {
                Console.WriteLine(
                  String.Format(
                    "Only {0} byte(s) of data could be read from the stream.",
                    len
                  )
                );
            }

            if (len <= WebSocket.FragmentLength)
                BroadcastAsync(Opcode.Binary, bytes, completed);
            else
                BroadcastAsync(Opcode.Binary, new MemoryStream(bytes), completed);
        }

        /// <summary>
        /// 关闭指定的会话
        /// </summary>
        /// <param name="id"></param>
        public void CloseSession(string id)
        {
            IControllerSession session;
            if (!TryGetSession(id, out session))
            {
                var msg = "The session could not be found.";
                throw new InvalidOperationException(msg);
            }

            session.Context.WebSocket.Close();
        }

        /// <summary>
        /// 关闭指定的会话
        /// </summary>
        /// <param name="id"></param>
        /// <param name="code"></param>
        /// <param name="reason"></param>
        public void CloseSession(string id, ushort code, string reason)
        {
            IControllerSession session;
            if (!TryGetSession(id, out session))
            {
                var msg = "The session could not be found.";
                throw new InvalidOperationException(msg);
            }

            session.Context.WebSocket.Close(code, reason);
        }

        /// <summary>
        /// 关闭指定的会话
        /// </summary>
        /// <param name="id"></param>
        /// <param name="code"></param>
        /// <param name="reason"></param>
        public void CloseSession(string id, CloseStatusCode code, string reason)
        {
            IControllerSession session;
            if (!TryGetSession(id, out session))
            {
                var msg = "The session could not be found.";
                throw new InvalidOperationException(msg);
            }

            session.Context.WebSocket.Close(code, reason);
        }

        /// <summary>
        /// 使用指定的会话向客户端发送 ping
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool PingTo(string id)
        {
            IControllerSession session;
            if (!TryGetSession(id, out session))
            {
                var msg = "The session could not be found.";
                throw new InvalidOperationException(msg);
            }

            return session.Context.WebSocket.Ping();
        }

        /// <summary>
        /// 使用指定的会话向客户端发送 ping
        /// </summary>
        /// <param name="message"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool PingTo(string message, string id)
        {
            IControllerSession session;
            if (!TryGetSession(id, out session))
            {
                var msg = "The session could not be found.";
                throw new InvalidOperationException(msg);
            }

            return session.Context.WebSocket.Ping(message);
        }

        /// <summary>
        /// 使用指定的会话向客户端发送数据
        /// </summary>
        /// <param name="data"></param>
        /// <param name="id"></param>
        public void SendTo(byte[] data, string id)
        {
            IControllerSession session;
            if (!TryGetSession(id, out session))
            {
                var msg = "The session could not be found.";
                throw new InvalidOperationException(msg);
            }

            session.Context.WebSocket.Send(data);
        }

        /// <summary>
        /// 使用指定的会话向客户端发送数据
        /// </summary>
        /// <param name="data"></param>
        /// <param name="id"></param>
        public void SendTo(string data, string id)
        {
            IControllerSession session;
            if (!TryGetSession(id, out session))
            {
                var msg = "The session could not be found.";
                throw new InvalidOperationException(msg);
            }

            session.Context.WebSocket.Send(data);
        }

        /// <summary>
        /// 使用指定的会话向客户端发送数据
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="length"></param>
        /// <param name="id"></param>
        public void SendTo(Stream stream, int length, string id)
        {
            IControllerSession session;
            if (!TryGetSession(id, out session))
            {
                var msg = "The session could not be found.";
                throw new InvalidOperationException(msg);
            }

            session.Context.WebSocket.Send(stream, length);
        }

        /// <summary>
        /// 使用指定的会话向客户端发送数据
        /// </summary>
        /// <param name="data"></param>
        /// <param name="id"></param>
        /// <param name="completed"></param>
        public void SendToAsync(byte[] data, string id, Action<bool> completed)
        {
            IControllerSession session;
            if (!TryGetSession(id, out session))
            {
                var msg = "The session could not be found.";
                throw new InvalidOperationException(msg);
            }

            session.Context.WebSocket.SendAsync(data, completed);
        }

        /// <summary>
        /// 使用指定的会话向客户端发送数据
        /// </summary>
        /// <param name="data"></param>
        /// <param name="id"></param>
        /// <param name="completed"></param>
        public void SendToAsync(string data, string id, Action<bool> completed)
        {
            IControllerSession session;
            if (!TryGetSession(id, out session))
            {
                var msg = "The session could not be found.";
                throw new InvalidOperationException(msg);
            }

            session.Context.WebSocket.SendAsync(data, completed);
        }

        /// <summary>
        /// 使用指定的会话向客户端发送数据
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="length"></param>
        /// <param name="id"></param>
        /// <param name="completed"></param>
        public void SendToAsync(
          Stream stream, int length, string id, Action<bool> completed
        )
        {
            IControllerSession session;
            if (!TryGetSession(id, out session))
            {
                var msg = "The session could not be found.";
                throw new InvalidOperationException(msg);
            }

            session.Context.WebSocket.SendAsync(stream, length, completed);
        }

        /// <summary>
        /// 清除 WebSocket 服务中的非活动会话。
        /// </summary>
        public void Sweep()
        {
            if (_sweeping)
            {
                Console.WriteLine("The sweeping is already in progress.");
                return;
            }

            lock (_forSweep)
            {
                if (_sweeping)
                {
                    Console.WriteLine("The sweeping is already in progress.");
                    return;
                }

                _sweeping = true;
            }

            foreach (var id in InactiveIDs)
            {
                if (_state != ServerState.Start)
                    break;

                lock (_sync)
                {
                    if (_state != ServerState.Start)
                        break;

                    IControllerSession session;
                    if (_sessions.TryGetValue(id, out session))
                    {
                        var state = session.ConnectionState;
                        if (state == WebSocketState.Open)
                            session.Context.WebSocket.Close(CloseStatusCode.Abnormal);
                        else if (state == WebSocketState.Closing)
                            continue;
                        else
                            _sessions.Remove(id);
                    }
                }
            }

            _sweeping = false;
        }

        /// <summary>
        /// 尝试获取会话实例
        /// </summary>
        /// <param name="id"></param>
        /// <param name="session"></param>
        /// <returns></returns>
        public bool TryGetSession(string id, out IControllerSession session)
        {
            if (id == null)
                throw new ArgumentNullException("id");

            if (id.Length == 0)
                throw new ArgumentException("An empty string.", "id");

            return _TryGetSession(id, out session);
        }
    }
}
