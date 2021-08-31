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
    public class WebSocketSessionManager
    {
        #region Private Fields

        private volatile bool _clean;
        private object _forSweep;
        private Dictionary<string, IWebSocketSession> _sessions;
        private volatile ServerState _state;
        private volatile bool _sweeping;
        private System.Timers.Timer _sweepTimer;
        private object _sync;
        private TimeSpan _waitTime;

        #endregion

        #region Internal Constructors

        internal WebSocketSessionManager()
        {
            _clean = true;
            _forSweep = new object();
            _sessions = new Dictionary<string, IWebSocketSession>();
            _state = ServerState.Ready;
            _sync = ((System.Collections.ICollection)_sessions).SyncRoot;
            _waitTime = TimeSpan.FromSeconds(1);

            SetSweepTimer(60000);
        }

        #endregion

        #region Internal Properties

        internal ServerState State
        {
            get
            {
                return _state;
            }
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the IDs for the active sessions in the WebSocket service.
        /// </summary>
        /// <value>
        ///   <para>
        ///   An <c>IEnumerable&lt;string&gt;</c> instance.
        ///   </para>
        ///   <para>
        ///   It provides an enumerator which supports the iteration over
        ///   the collection of the IDs for the active sessions.
        ///   </para>
        /// </value>
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

        /// <summary>
        /// Gets the number of the sessions in the WebSocket service.
        /// </summary>
        /// <value>
        /// An <see cref="int"/> that represents the number of the sessions.
        /// </value>
        public int Count
        {
            get
            {
                lock (_sync)
                    return _sessions.Count;
            }
        }

        /// <summary>
        /// Gets the IDs for the sessions in the WebSocket service.
        /// </summary>
        /// <value>
        ///   <para>
        ///   An <c>IEnumerable&lt;string&gt;</c> instance.
        ///   </para>
        ///   <para>
        ///   It provides an enumerator which supports the iteration over
        ///   the collection of the IDs for the sessions.
        ///   </para>
        /// </value>
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
        /// Gets the IDs for the inactive sessions in the WebSocket service.
        /// </summary>
        /// <value>
        ///   <para>
        ///   An <c>IEnumerable&lt;string&gt;</c> instance.
        ///   </para>
        ///   <para>
        ///   It provides an enumerator which supports the iteration over
        ///   the collection of the IDs for the inactive sessions.
        ///   </para>
        /// </value>
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
        public IWebSocketSession this[string id]
        {
            get
            {
                if (id == null)
                    throw new ArgumentNullException("id");

                if (id.Length == 0)
                    throw new ArgumentException("An empty string.", "id");

                IWebSocketSession session;
                _TryGetSession(id, out session);

                return session;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the inactive sessions in
        /// the WebSocket service are cleaned up periodically.
        /// </summary>
        /// <remarks>
        /// The set operation does nothing if the service has already started or
        /// it is shutting down.
        /// </remarks>
        /// <value>
        /// <c>true</c> if the inactive sessions are cleaned up every 60 seconds;
        /// otherwise, <c>false</c>.
        /// </value>
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
        /// Gets the session instances in the WebSocket service.
        /// </summary>
        /// <value>
        ///   <para>
        ///   An <c>IEnumerable&lt;IWebSocketSession&gt;</c> instance.
        ///   </para>
        ///   <para>
        ///   It provides an enumerator which supports the iteration over
        ///   the collection of the session instances.
        ///   </para>
        /// </value>
        public IEnumerable<IWebSocketSession> Sessions
        {
            get
            {
                if (_state != ServerState.Start)
                    return Enumerable.Empty<IWebSocketSession>();

                lock (_sync)
                {
                    if (_state != ServerState.Start)
                        return Enumerable.Empty<IWebSocketSession>();

                    return ExtensionMethod.ToList(_sessions.Values);
                }
            }
        }

        /// <summary>
        /// Gets or sets the time to wait for the response to the WebSocket Ping or
        /// Close.
        /// </summary>
        /// <remarks>
        /// The set operation does nothing if the service has already started or
        /// it is shutting down.
        /// </remarks>
        /// <value>
        /// A <see cref="TimeSpan"/> to wait for the response.
        /// </value>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The value specified for a set operation is zero or less.
        /// </exception>
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

        #endregion

        #region Private Methods

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

        private bool _TryGetSession(string id, out IWebSocketSession session)
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

        #endregion

        #region Internal Methods

        internal string Add(IWebSocketSession session)
        {
            lock (_sync)
            {
                if (_state != ServerState.Start)
                    return null;

                var id = CreateID();
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

        #endregion

        #region Public Methods

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
        /// Sends <paramref name="data"/> to every client in the WebSocket service.
        /// </summary>
        /// <param name="data">
        /// A <see cref="string"/> that represents the text data to send.
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// The current state of the manager is not Start.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="data"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="data"/> could not be UTF-8-encoded.
        /// </exception>
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
        /// Sends the data from <paramref name="stream"/> to every client in
        /// the WebSocket service.
        /// </summary>
        /// <remarks>
        /// The data is sent as the binary data.
        /// </remarks>
        /// <param name="stream">
        /// A <see cref="Stream"/> instance from which to read the data to send.
        /// </param>
        /// <param name="length">
        /// An <see cref="int"/> that specifies the number of bytes to send.
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// The current state of the manager is not Start.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="stream"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///   <para>
        ///   <paramref name="stream"/> cannot be read.
        ///   </para>
        ///   <para>
        ///   -or-
        ///   </para>
        ///   <para>
        ///   <paramref name="length"/> is less than 1.
        ///   </para>
        ///   <para>
        ///   -or-
        ///   </para>
        ///   <para>
        ///   No data could be read from <paramref name="stream"/>.
        ///   </para>
        /// </exception>
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
        /// Sends <paramref name="data"/> asynchronously to every client in
        /// the WebSocket service.
        /// </summary>
        /// <remarks>
        /// This method does not wait for the send to be complete.
        /// </remarks>
        /// <param name="data">
        /// An array of <see cref="byte"/> that represents the binary data to send.
        /// </param>
        /// <param name="completed">
        ///   <para>
        ///   An <see cref="Action"/> delegate or <see langword="null"/>
        ///   if not needed.
        ///   </para>
        ///   <para>
        ///   The delegate invokes the method called when the send is complete.
        ///   </para>
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// The current state of the manager is not Start.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="data"/> is <see langword="null"/>.
        /// </exception>
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
        /// Sends <paramref name="data"/> asynchronously to every client in
        /// the WebSocket service.
        /// </summary>
        /// <remarks>
        /// This method does not wait for the send to be complete.
        /// </remarks>
        /// <param name="data">
        /// A <see cref="string"/> that represents the text data to send.
        /// </param>
        /// <param name="completed">
        ///   <para>
        ///   An <see cref="Action"/> delegate or <see langword="null"/>
        ///   if not needed.
        ///   </para>
        ///   <para>
        ///   The delegate invokes the method called when the send is complete.
        ///   </para>
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// The current state of the manager is not Start.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="data"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="data"/> could not be UTF-8-encoded.
        /// </exception>
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
        /// Sends the data from <paramref name="stream"/> asynchronously to
        /// every client in the WebSocket service.
        /// </summary>
        /// <remarks>
        ///   <para>
        ///   The data is sent as the binary data.
        ///   </para>
        ///   <para>
        ///   This method does not wait for the send to be complete.
        ///   </para>
        /// </remarks>
        /// <param name="stream">
        /// A <see cref="Stream"/> instance from which to read the data to send.
        /// </param>
        /// <param name="length">
        /// An <see cref="int"/> that specifies the number of bytes to send.
        /// </param>
        /// <param name="completed">
        ///   <para>
        ///   An <see cref="Action"/> delegate or <see langword="null"/>
        ///   if not needed.
        ///   </para>
        ///   <para>
        ///   The delegate invokes the method called when the send is complete.
        ///   </para>
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// The current state of the manager is not Start.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="stream"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///   <para>
        ///   <paramref name="stream"/> cannot be read.
        ///   </para>
        ///   <para>
        ///   -or-
        ///   </para>
        ///   <para>
        ///   <paramref name="length"/> is less than 1.
        ///   </para>
        ///   <para>
        ///   -or-
        ///   </para>
        ///   <para>
        ///   No data could be read from <paramref name="stream"/>.
        ///   </para>
        /// </exception>
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
        /// Sends a ping to every client in the WebSocket service.
        /// </summary>
        /// <returns>
        ///   <para>
        ///   A <c>Dictionary&lt;string, bool&gt;</c>.
        ///   </para>
        ///   <para>
        ///   It represents a collection of pairs of a session ID and
        ///   a value indicating whether a pong has been received from
        ///   the client within a time.
        ///   </para>
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// The current state of the manager is not Start.
        /// </exception>
        [Obsolete("This method will be removed.")]
        public Dictionary<string, bool> Broadping()
        {
            if (_state != ServerState.Start)
            {
                var msg = "The current state of the manager is not Start.";
                throw new InvalidOperationException(msg);
            }

            return Broadping(WebSocketFrame.EmptyPingBytes, _waitTime);
        }

        /// <summary>
        /// Sends a ping with <paramref name="message"/> to every client in
        /// the WebSocket service.
        /// </summary>
        /// <returns>
        ///   <para>
        ///   A <c>Dictionary&lt;string, bool&gt;</c>.
        ///   </para>
        ///   <para>
        ///   It represents a collection of pairs of a session ID and
        ///   a value indicating whether a pong has been received from
        ///   the client within a time.
        ///   </para>
        /// </returns>
        /// <param name="message">
        ///   <para>
        ///   A <see cref="string"/> that represents the message to send.
        ///   </para>
        ///   <para>
        ///   The size must be 125 bytes or less in UTF-8.
        ///   </para>
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// The current state of the manager is not Start.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="message"/> could not be UTF-8-encoded.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The size of <paramref name="message"/> is greater than 125 bytes.
        /// </exception>
        [Obsolete("This method will be removed.")]
        public Dictionary<string, bool> Broadping(string message)
        {
            if (_state != ServerState.Start)
            {
                var msg = "The current state of the manager is not Start.";
                throw new InvalidOperationException(msg);
            }

            if (message.IsNullOrEmpty())
                return Broadping(WebSocketFrame.EmptyPingBytes, _waitTime);

            byte[] bytes;
            if (!message.TryGetUTF8EncodedBytes(out bytes))
            {
                var msg = "It could not be UTF-8-encoded.";
                throw new ArgumentException(msg, "message");
            }

            if (bytes.Length > 125)
            {
                var msg = "Its size is greater than 125 bytes.";
                throw new ArgumentOutOfRangeException("message", msg);
            }

            var frame = WebSocketFrame.CreatePingFrame(bytes, false);
            return Broadping(frame.ToArray(), _waitTime);
        }

        /// <summary>
        /// Closes the specified session.
        /// </summary>
        /// <param name="id">
        /// A <see cref="string"/> that represents the ID of the session to close.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="id"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="id"/> is an empty string.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The session could not be found.
        /// </exception>
        public void CloseSession(string id)
        {
            IWebSocketSession session;
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
            IWebSocketSession session;
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
            IWebSocketSession session;
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
            IWebSocketSession session;
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
            IWebSocketSession session;
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
            IWebSocketSession session;
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
            IWebSocketSession session;
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
            IWebSocketSession session;
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
            IWebSocketSession session;
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
            IWebSocketSession session;
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
            IWebSocketSession session;
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

                    IWebSocketSession session;
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
        public bool TryGetSession(string id, out IWebSocketSession session)
        {
            if (id == null)
                throw new ArgumentNullException("id");

            if (id.Length == 0)
                throw new ArgumentException("An empty string.", "id");

            return _TryGetSession(id, out session);
        }

        #endregion
    }
}
