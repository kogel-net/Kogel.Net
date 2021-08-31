using Kogel.Net.WebSocket.Enums;
using Kogel.Net.WebSocket.Extension;
using Kogel.Net.WebSocket.Extension.Net;
using Kogel.Net.WebSocket.Extension.Net.WebSockets;
using Kogel.Net.WebSocket.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kogel.Net.WebSocket.Server
{
    /// <summary>
    /// 公开一组用于定义行为的方法和属性由或提供的 WebSocket 服务
    /// </summary>
    public abstract class WebSocketBehavior : IWebSocketSession
    {
        private WebSocketContext _context;
        private Func<CookieCollection, CookieCollection, bool> _cookiesValidator;
        private bool _emitOnPing;
        private string _id;
        private bool _ignoreExtensions;
        private Func<string, bool> _originValidator;
        private string _protocol;
        private WebSocketSessionManager _sessions;
        private DateTime _startTime;
        private WebSocket _websocket;

        /// <summary>
        /// 
        /// </summary>
        protected WebSocketBehavior()
        {
            _startTime = DateTime.MaxValue;
        }

        /// <summary>
        /// 获取包含在 WebSocket 握手请求中的 HTTP 标头
        /// </summary>
        protected NameValueCollection Headers
        {
            get
            {
                return _context != null ? _context.Headers : null;
            }
        }

        /// <summary>
        /// 获取包含在 WebSocket 握手请求中的查询字符串
        /// </summary>
        protected NameValueCollection QueryString
        {
            get
            {
                return _context != null ? _context.QueryString : null;
            }
        }

        /// <summary>
        /// 获取服务中会话的管理函数。
        /// </summary>
        protected WebSocketSessionManager Sessions
        {
            get
            {
                return _sessions;
            }
        }

        /// <summary>
        /// 获取会话的 WebSocket 连接的当前状态。
        /// </summary>
        public WebSocketState ConnectionState
        {
            get
            {
                return _websocket != null
                       ? _websocket.ReadyState
                       : WebSocketState.Connecting;
            }
        }

        /// <summary>
        /// 获取服务的 WebSocket 握手请求中的信息。
        /// </summary>
        public WebSocketContext Context
        {
            get
            {
                return _context;
            }
        }

        /// <summary>
        /// 获取或设置用于验证服务的 WebSocket 握手请求中包含的 HTTP cookie 的委托
        /// </summary>
        public Func<CookieCollection, CookieCollection, bool> CookiesValidator
        {
            get
            {
                return _cookiesValidator;
            }

            set
            {
                _cookiesValidator = value;
            }
        }

        /// <summary>
        /// 获取或设置一个值，该值指示会话的 WebSocket 实例在收到 ping 时是否发出消息事件
        /// </summary>
        public bool EmitOnPing
        {
            get
            {
                return _websocket != null ? _websocket.EmitOnPing : _emitOnPing;
            }

            set
            {
                if (_websocket != null)
                {
                    _websocket.EmitOnPing = value;
                    return;
                }

                _emitOnPing = value;
            }
        }

        /// <summary>
        /// 获取会话的唯一 ID
        /// </summary>
        public string Id
        {
            get
            {
                return _id;
            }
        }

        /// <summary>
        /// 获取或设置一个值，该值指示服务是否忽略包含在 WebSocket 握手请求中的 Sec-WebSocket-Extensions 标头。
        /// </summary>
        public bool IgnoreExtensions
        {
            get
            {
                return _ignoreExtensions;
            }

            set
            {
                _ignoreExtensions = value;
            }
        }

        /// <summary>
        /// 获取或设置用于验证对服务的 WebSocket 握手请求中包含的 Origin 标头的委托。
        /// </summary>
        public Func<string, bool> OriginValidator
        {
            get
            {
                return _originValidator;
            }

            set
            {
                _originValidator = value;
            }
        }

        /// <summary>
        /// 获取或设置服务的 WebSocket 子协议的名称。
        /// </summary>
        public string Protocol
        {
            get
            {
                return _websocket != null
                       ? _websocket.Protocol
                       : (_protocol ?? String.Empty);
            }

            set
            {
                if (ConnectionState != WebSocketState.Connecting)
                {
                    var msg = "The session has already started.";
                    throw new InvalidOperationException(msg);
                }

                if (value == null || value.Length == 0)
                {
                    _protocol = null;
                    return;
                }

                if (!value.IsToken())
                    throw new ArgumentException("Not a token.", "value");

                _protocol = value;
            }
        }

        /// <summary>
        /// 获取会话开始的时间。
        /// </summary>
        public DateTime StartTime
        {
            get
            {
                return _startTime;
            }
        }

        private string CheckHandshakeRequest(WebSocketContext context)
        {
            if (_originValidator != null)
            {
                if (!_originValidator(context.Origin))
                    return "It includes no Origin header or an invalid one.";
            }

            if (_cookiesValidator != null)
            {
                var req = context.CookieCollection;
                var res = context.WebSocket.CookieCollection;
                if (!_cookiesValidator(req, res))
                    return "It includes no cookie or an invalid one.";
            }

            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnClose(object sender, CloseEventArgs e)
        {
            if (_id == null)
                return;

            _sessions.Remove(_id);
            OnClose(e);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnError(object sender, Kogel.Net.WebSocket.Extension.ErrorEventArgs e)
        {
            OnError(e);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMessage(object sender, MessageEventArgs e)
        {
            OnMessage(e);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnOpen(object sender, EventArgs e)
        {
            _id = _sessions.Add(this);
            if (_id == null)
            {
                _websocket.Close(CloseStatusCode.Away);
                return;
            }

            _startTime = DateTime.Now;
            OnOpen();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="sessions"></param>
        internal void Start(WebSocketContext context, WebSocketSessionManager sessions)
        {
            if (_websocket != null)
            {
                Console.WriteLine("A session instance cannot be reused.");
                context.WebSocket.Close(HttpStatusCode.ServiceUnavailable);

                return;
            }

            _context = context;
            _sessions = sessions;

            _websocket = context.WebSocket;
            _websocket.CustomHandshakeRequestChecker = CheckHandshakeRequest;
            _websocket.EmitOnPing = _emitOnPing;
            _websocket.IgnoreExtensions = _ignoreExtensions;
            _websocket.Protocol = _protocol;

            var waitTime = sessions.WaitTime;
            if (waitTime != _websocket.WaitTime)
                _websocket.WaitTime = waitTime;

            _websocket.OnOpen += OnOpen;
            _websocket.OnMessage += OnMessage;
            _websocket.OnError += OnError;
            _websocket.OnClose += OnClose;

            _websocket.InternalAccept();
        }

        /// <summary>
        /// 闭会话的 WebSocket 连接。
        /// </summary>
        protected void Close()
        {
            if (_websocket == null)
            {
                var msg = "The session has not started yet.";
                throw new InvalidOperationException(msg);
            }

            _websocket.Close();
        }

        /// <summary>
        /// 使用指定的代码和原因关闭会话的 WebSocket 连接。
        /// </summary>
        /// <param name="code"></param>
        /// <param name="reason"></param>
        protected void Close(ushort code, string reason)
        {
            if (_websocket == null)
            {
                var msg = "The session has not started yet.";
                throw new InvalidOperationException(msg);
            }

            _websocket.Close(code, reason);
        }

        /// <summary>
        /// 用指定的代码和原因关闭会话的 WebSocket 连接。
        /// </summary>
        /// <param name="code"></param>
        /// <param name="reason"></param>
        protected void Close(CloseStatusCode code, string reason)
        {
            if (_websocket == null)
            {
                var msg = "The session has not started yet.";
                throw new InvalidOperationException(msg);
            }

            _websocket.Close(code, reason);
        }

        /// <summary>
        /// 异步关闭会话的 WebSocket 连接。
        /// </summary>
        protected void CloseAsync()
        {
            if (_websocket == null)
            {
                var msg = "The session has not started yet.";
                throw new InvalidOperationException(msg);
            }

            _websocket.CloseAsync();
        }

        /// <summary>
        /// 使用指定的代码和原因异步关闭会话的 WebSocket 连接。
        /// </summary>
        /// <param name="code"></param>
        /// <param name="reason"></param>
        protected void CloseAsync(ushort code, string reason)
        {
            if (_websocket == null)
            {
                var msg = "The session has not started yet.";
                throw new InvalidOperationException(msg);
            }

            _websocket.CloseAsync(code, reason);
        }

        /// <summary>
        /// 使用指定的代码和原因异步关闭会话的 WebSocket 连接。
        /// </summary>
        /// <param name="code"></param>
        /// <param name="reason"></param>
        protected void CloseAsync(CloseStatusCode code, string reason)
        {
            if (_websocket == null)
            {
                var msg = "The session has not started yet.";
                throw new InvalidOperationException(msg);
            }

            _websocket.CloseAsync(code, reason);
        }

        /// <summary>
        /// 当会话的 WebSocket 连接关闭时调用。
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnClose(CloseEventArgs e)
        {
        }

        /// <summary>
        /// 当会话的 WebSocket 实例出错时调用。
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnError(Kogel.Net.WebSocket.Extension.ErrorEventArgs e)
        {
        }

        /// <summary>
        /// 当会话的 WebSocket 实例收到消息时调用。
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnMessage(MessageEventArgs e)
        {
        }

        /// <summary>
        /// 在为会话建立 WebSocket 连接时调用。
        /// </summary>
        protected virtual void OnOpen()
        {
        }

        /// <summary>
        /// 使用 WebSocket 连接将指定数据发送到客户端。
        /// </summary>
        /// <param name="data"></param>
        protected void Send(byte[] data)
        {
            if (_websocket == null)
            {
                var msg = "The current state of the connection is not Open.";
                throw new InvalidOperationException(msg);
            }

            _websocket.Send(data);
        }

        /// <summary>
        /// 使用 WebSocket 连接将指定文件发送到客户端
        /// </summary>
        /// <param name="fileInfo"></param>
        protected void Send(FileInfo fileInfo)
        {
            if (_websocket == null)
            {
                var msg = "The current state of the connection is not Open.";
                throw new InvalidOperationException(msg);
            }

            _websocket.Send(fileInfo);
        }

        /// <summary>
        /// 使用 WebSocket 连接将指定数据发送到客户端。
        /// </summary>
        /// <param name="data"></param>
        protected void Send(string data)
        {
            if (_websocket == null)
            {
                var msg = "The current state of the connection is not Open.";
                throw new InvalidOperationException(msg);
            }

            _websocket.Send(data);
        }

        /// <summary>
        /// 使用指定的流将数据发送到客户端
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="length"></param>
        protected void Send(Stream stream, int length)
        {
            if (_websocket == null)
            {
                var msg = "The current state of the connection is not Open.";
                throw new InvalidOperationException(msg);
            }

            _websocket.Send(stream, length);
        }

        /// <summary>
        /// 使用 WebSocket 连接将指定数据异步发送到客户端。
        /// </summary>
        /// <param name="data"></param>
        /// <param name="completed"></param>
        protected void SendAsync(byte[] data, Action<bool> completed)
        {
            if (_websocket == null)
            {
                var msg = "The current state of the connection is not Open.";
                throw new InvalidOperationException(msg);
            }

            _websocket.SendAsync(data, completed);
        }

        /// <summary>
        /// 使用 WebSocket 连接将指定文件异步发送到客户端。
        /// </summary>
        /// <param name="fileInfo"></param>
        /// <param name="completed"></param>
        protected void SendAsync(FileInfo fileInfo, Action<bool> completed)
        {
            if (_websocket == null)
            {
                var msg = "The current state of the connection is not Open.";
                throw new InvalidOperationException(msg);
            }

            _websocket.SendAsync(fileInfo, completed);
        }

        /// <summary>
        /// 使用 WebSocket 连接将指定数据异步发送到客户端。
        /// </summary>
        /// <param name="data"></param>
        /// <param name="completed"></param>
        protected void SendAsync(string data, Action<bool> completed)
        {
            if (_websocket == null)
            {
                var msg = "The current state of the connection is not Open.";
                throw new InvalidOperationException(msg);
            }

            _websocket.SendAsync(data, completed);
        }

        /// <summary>
        /// 使用 WebSocket 连接将数据从指定流异步发送到客户端。
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="length"></param>
        /// <param name="completed"></param>
        protected void SendAsync(Stream stream, int length, Action<bool> completed)
        {
            if (_websocket == null)
            {
                var msg = "The current state of the connection is not Open.";
                throw new InvalidOperationException(msg);
            }

            _websocket.SendAsync(stream, length, completed);
        }
    }
}
