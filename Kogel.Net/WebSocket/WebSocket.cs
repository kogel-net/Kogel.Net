using Kogel.Net.WebSocket.Enums;
using Kogel.Net.WebSocket.Extension;
using Kogel.Net.WebSocket.Extension.Net;
using Kogel.Net.WebSocket.Extension.Net.WebSockets;
using Kogel.Net.WebSocket.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Kogel.Net.WebSocket
{
    /// <summary>
    /// 长连接对象
    /// </summary>
    public class WebSocket : IDisposable
    {
        private AuthenticationChallenge _authChallenge;
        private string _base64Key;
        private bool _client;
        private Action _closeContext;
        private CompressionMethod _compression;
        private WebSocketContext _context;
        private CookieCollection _cookies;
        private NetworkCredential _credentials;
        private bool _emitOnPing;
        private bool _enableRedirection;
        private string _extensions;
        private bool _extensionsRequested;
        private object _forMessageEventQueue;
        private object _forPing;
        private object _forSend;
        private object _forState;
        private MemoryStream _fragmentsBuffer;
        private bool _fragmentsCompressed;
        private Opcode _fragmentsOpcode;
        private const string _guid = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";
        private Func<WebSocketContext, string> _handshakeRequestChecker;
        private bool _ignoreExtensions;
        private bool _inContinuation;
        private volatile bool _inMessage;
        private static readonly int _maxRetryCountForConnect;
        private Action<MessageEventArgs> _message;
        private Queue<MessageEventArgs> _messageEventQueue;
        private uint _nonceCount;
        private string _origin;
        private ManualResetEvent _pongReceived;
        private bool _preAuth;
        private string _protocol;
        private string[] _protocols;
        private bool _protocolsRequested;
        private NetworkCredential _proxyCredentials;
        private Uri _proxyUri;
        private volatile WebSocketState _readyState;
        private ManualResetEvent _receivingExited;
        private int _retryCountForConnect;
        private bool _secure;
        private ClientSslConfiguration _sslConfig;
        private Stream _stream;
        private TcpClient _tcpClient;
        private Uri _uri;
        private const string _version = "13";
        private TimeSpan _waitTime;

        /// <summary>
        /// 表示内部使用的 <see cref="byte"/> 的空数组
        /// </summary>
        internal static readonly byte[] EmptyBytes;

        /// <summary>
        /// 表示用于确定发送时数据是否应该分片的长度
        /// </summary>
        internal static readonly int FragmentLength;

        /// <summary>
        /// 代表内部使用的随机数生成器
        /// </summary>
        internal static readonly RandomNumberGenerator RandomNumber;

        static WebSocket()
        {
            _maxRetryCountForConnect = 10;
            EmptyBytes = new byte[0];
            FragmentLength = 1016;
            RandomNumber = new RNGCryptoServiceProvider();
        }

        internal WebSocket(HttpListenerWebSocketContext context, string protocol)
        {
            _context = context;
            _protocol = protocol;

            _closeContext = context.Close;
            _message = Messages;
            _secure = context.IsSecureConnection;
            _stream = context.Stream;
            _waitTime = TimeSpan.FromSeconds(1);
            Init();
        }

        internal WebSocket(TcpListenerWebSocketContext context, string protocol)
        {
            _context = context;
            _protocol = protocol;

            _closeContext = context.Close;
            _message = Messages;
            _secure = context.IsSecureConnection;
            _stream = context.Stream;
            _waitTime = TimeSpan.FromSeconds(1);

            Init();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <param name="protocols"></param>
        public WebSocket(string url, params string[] protocols)
        {
            if (url == null)
                throw new ArgumentNullException("url");

            if (url.Length == 0)
                throw new ArgumentException("An empty string.", "url");

            string msg;
            if (!url.TryCreateWebSocketUri(out _uri, out msg))
                throw new ArgumentException(msg, "url");

            if (protocols != null && protocols.Length > 0)
            {
                if (!CheckProtocols(protocols, out msg))
                    throw new ArgumentException(msg, "protocols");

                _protocols = protocols;
            }

            _base64Key = CreateBase64Key();
            _client = true;
            _message = Messagec;
            _secure = _uri.Scheme == "wss";
            _waitTime = TimeSpan.FromSeconds(5);

            Init();
        }

        internal CookieCollection CookieCollection
        {
            get
            {
                return _cookies;
            }
        }

        internal Func<WebSocketContext, string> CustomHandshakeRequestChecker
        {
            get
            {
                return _handshakeRequestChecker;
            }

            set
            {
                _handshakeRequestChecker = value;
            }
        }

        internal bool HasMessage
        {
            get
            {
                lock (_forMessageEventQueue)
                    return _messageEventQueue.Count > 0;
            }
        }

        internal bool IgnoreExtensions
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

        internal bool IsConnected
        {
            get
            {
                return _readyState == WebSocketState.Open || _readyState == WebSocketState.Closing;
            }
        }

        /// <summary>
        /// 获取或设置用于压缩消息的压缩方法
        /// </summary>
        public CompressionMethod Compression
        {
            get
            {
                return _compression;
            }

            set
            {
                string msg = null;

                if (!_client)
                {
                    msg = "This instance is not a client.";
                    throw new InvalidOperationException(msg);
                }

                lock (_forState)
                {
                    if (!CanSet(out msg))
                    {
                        return;
                    }

                    _compression = value;
                }
            }
        }

        /// <summary>
        /// 获取包含在握手请求/响应中的 HTTP cookie
        /// </summary>
        public IEnumerable<Cookie> Cookies
        {
            get
            {
                lock (_cookies.SyncRoot)
                {
                    foreach (Cookie cookie in _cookies)
                        yield return cookie;
                }
            }
        }

        /// <summary>
        /// 获取包含在握手请求/响应中的 HTTP cookie
        /// </summary>
        public NetworkCredential Credentials
        {
            get
            {
                return _credentials;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool EmitOnPing
        {
            get
            {
                return _emitOnPing;
            }

            set
            {
                _emitOnPing = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool EnableRedirection
        {
            get
            {
                return _enableRedirection;
            }

            set
            {
                string msg = null;

                if (!_client)
                {
                    msg = "This instance is not a client.";
                    throw new InvalidOperationException(msg);
                }

                if (!CanSet(out msg))
                {
                    return;
                }

                lock (_forState)
                {
                    if (!CanSet(out msg))
                    {
                        return;
                    }

                    _enableRedirection = value;
                }
            }
        }

        /// <summary>
        /// 获取服务器选择的扩展
        /// </summary>
        public string Extensions
        {
            get
            {
                return _extensions ?? String.Empty;
            }
        }

        /// <summary>
        /// 获取一个值，该值指示连接是否处于活动状态
        /// </summary>
        public bool IsAlive
        {
            get
            {
                return Ping(EmptyBytes);
            }
        }

        /// <summary>
        /// 获取指示是否使用安全连接的值
        /// </summary>
        public bool IsSecure
        {
            get
            {
                return _secure;
            }
        }

        /// <summary>
        /// 获取或设置要发送的 HTTP 源头的值
        /// </summary>
        public string Origin
        {
            get
            {
                return _origin;
            }

            set
            {
                string msg = null;

                if (!_client)
                {
                    msg = "This instance is not a client.";
                    throw new InvalidOperationException(msg);
                }

                if (!value.IsNullOrEmpty())
                {
                    Uri uri;
                    if (!Uri.TryCreate(value, UriKind.Absolute, out uri))
                    {
                        msg = "Not an absolute URI string.";
                        throw new ArgumentException(msg, "value");
                    }

                    if (uri.Segments.Length > 1)
                    {
                        msg = "It includes the path segments.";
                        throw new ArgumentException(msg, "value");
                    }
                }

                if (!CanSet(out msg))
                {
                    return;
                }

                lock (_forState)
                {
                    if (!CanSet(out msg))
                    {
                        return;
                    }

                    _origin = !value.IsNullOrEmpty() ? value.TrimEnd('/') : value;
                }
            }
        }

        /// <summary>
        /// 获取服务器选择的子协议的名称
        /// </summary>
        public string Protocol
        {
            get
            {
                return _protocol ?? String.Empty;
            }

            internal set
            {
                _protocol = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public WebSocketState ReadyState
        {
            get
            {
                return _readyState;
            }
        }

        /// <summary>
        /// 获取ssl配置
        /// </summary>
        public ClientSslConfiguration SslConfiguration
        {
            get
            {
                if (!_client)
                {
                    var msg = "This instance is not a client.";
                    throw new InvalidOperationException(msg);
                }

                if (!_secure)
                {
                    var msg = "This instance does not use a secure connection.";
                    throw new InvalidOperationException(msg);
                }

                return GetSslConfiguration();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public Uri Url
        {
            get
            {
                return _client ? _uri : _context.RequestUri;
            }
        }

        /// <summary>
        /// 获取或设置等待响应 ping 或关闭的时间。
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
                    return;
                }

                lock (_forState)
                {
                    if (!CanSet(out msg))
                    {
                        return;
                    }

                    _waitTime = value;
                }
            }
        }


        /// <summary>
        /// 当 WebSocket 连接关闭时发生
        /// </summary>
        public event EventHandler<CloseEventArgs> OnClose;

        /// <summary>
        /// 当 WebSocket 连接异常时发生
        /// </summary>
        public event EventHandler<Kogel.Net.WebSocket.Extension.ErrorEventArgs> OnError;

        /// <summary>
        /// 当 WebSocket 连接接收到消息时发生
        /// </summary>
        public event EventHandler<MessageEventArgs> OnMessage;

        /// <summary>
        /// 当 WebSocket 连接打开时发生
        /// </summary>
        public event EventHandler OnOpen;

        /// <summary>
        /// 接受服务器
        /// </summary>
        /// <returns></returns>
        private bool Accept()
        {
            if (_readyState == WebSocketState.Open)
            {
                var msg = "The handshake request has already been accepted.";
                Console.WriteLine(msg);
                return false;
            }

            lock (_forState)
            {
                if (_readyState == WebSocketState.Open)
                {
                    var msg = "The handshake request has already been accepted.";

                    return false;
                }

                if (_readyState == WebSocketState.Closing)
                {
                    var msg = "The close process has set in.";

                    msg = "An interruption has occurred while attempting to accept.";
                    Error(msg, null);

                    return false;
                }

                if (_readyState == WebSocketState.Closed)
                {
                    var msg = "The connection has been closed.";

                    msg = "An interruption has occurred while attempting to accept.";
                    Error(msg, null);

                    return false;
                }

                try
                {
                    if (!AcceptHandshake())
                        return false;
                }
                catch (Exception ex)
                {
                    var msg = "An exception has occurred while attempting to accept.";
                    Fatal(msg, ex);

                    return false;
                }

                _readyState = WebSocketState.Open;
                return true;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private bool AcceptHandshake()
        {

            string msg;
            if (!CheckHandshakeRequest(_context, out msg))
            {
                RefuseHandshake(
                  CloseStatusCode.ProtocolError,
                  "A handshake error has occurred while attempting to accept."
                );

                return false;
            }

            if (!CustomCheckHandshakeRequest(_context, out msg))
            {
                RefuseHandshake(
                  CloseStatusCode.PolicyViolation,
                  "A handshake error has occurred while attempting to accept."
                );

                return false;
            }

            _base64Key = _context.Headers["Sec-WebSocket-Key"];

            if (_protocol != null)
            {
                var vals = _context.SecWebSocketProtocols;
                ProcessSecWebSocketProtocolClientHeader(vals);
            }

            if (!_ignoreExtensions)
            {
                var val = _context.Headers["Sec-WebSocket-Extensions"];
                ProcessSecWebSocketExtensionsClientHeader(val);
            }

            return sendHttpResponse(CreateHandshakeResponse());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private bool CanSet(out string message)
        {
            message = null;

            if (_readyState == WebSocketState.Open)
            {
                message = "The connection has already been established.";
                return false;
            }

            if (_readyState == WebSocketState.Closing)
            {
                message = "The connection is closing.";
                return false;
            }

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        private bool CheckHandshakeRequest(WebSocketContext context, out string message)
        {
            message = null;

            if (!context.IsWebSocketRequest)
            {
                message = "Not a handshake request.";
                return false;
            }

            if (context.RequestUri == null)
            {
                message = "It specifies an invalid Request-URI.";
                return false;
            }

            var headers = context.Headers;

            var key = headers["Sec-WebSocket-Key"];
            if (key == null)
            {
                message = "It includes no Sec-WebSocket-Key header.";
                return false;
            }

            if (key.Length == 0)
            {
                message = "It includes an invalid Sec-WebSocket-Key header.";
                return false;
            }

            var version = headers["Sec-WebSocket-Version"];
            if (version == null)
            {
                message = "It includes no Sec-WebSocket-Version header.";
                return false;
            }

            if (version != _version)
            {
                message = "It includes an invalid Sec-WebSocket-Version header.";
                return false;
            }

            var protocol = headers["Sec-WebSocket-Protocol"];
            if (protocol != null && protocol.Length == 0)
            {
                message = "It includes an invalid Sec-WebSocket-Protocol header.";
                return false;
            }

            if (!_ignoreExtensions)
            {
                var extensions = headers["Sec-WebSocket-Extensions"];
                if (extensions != null && extensions.Length == 0)
                {
                    message = "It includes an invalid Sec-WebSocket-Extensions header.";
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="response"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        private bool CheckHandshakeResponse(HttpResponse response, out string message)
        {
            message = null;

            if (response.IsRedirect)
            {
                message = "Indicates the redirection.";
                return false;
            }

            if (response.IsUnauthorized)
            {
                message = "Requires the authentication.";
                return false;
            }

            if (!response.IsWebSocketResponse)
            {
                message = "Not a WebSocket handshake response.";
                return false;
            }

            var headers = response.Headers;
            if (!validateSecWebSocketAcceptHeader(headers["Sec-WebSocket-Accept"]))
            {
                message = "Includes no Sec-WebSocket-Accept header, or it has an invalid value.";
                return false;
            }

            if (!validateSecWebSocketProtocolServerHeader(headers["Sec-WebSocket-Protocol"]))
            {
                message = "Includes no Sec-WebSocket-Protocol header, or it has an invalid value.";
                return false;
            }

            if (!validateSecWebSocketExtensionsServerHeader(headers["Sec-WebSocket-Extensions"]))
            {
                message = "Includes an invalid Sec-WebSocket-Extensions header.";
                return false;
            }

            if (!validateSecWebSocketVersionServerHeader(headers["Sec-WebSocket-Version"]))
            {
                message = "Includes an invalid Sec-WebSocket-Version header.";
                return false;
            }

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="protocols"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        private static bool CheckProtocols(string[] protocols, out string message)
        {
            message = null;

            Func<string, bool> cond = protocol => protocol.IsNullOrEmpty()
                                                  || !protocol.IsToken();

            if (protocols.Contains(cond))
            {
                message = "It contains a value that is not a token.";
                return false;
            }

            if (protocols.ContainsTwice())
            {
                message = "It contains a value twice.";
                return false;
            }

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="frame"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        private bool CheckReceivedFrame(WebSocketFrame frame, out string message)
        {
            message = null;

            var masked = frame.IsMasked;
            if (_client && masked)
            {
                message = "A frame from the server is masked.";
                return false;
            }

            if (!_client && !masked)
            {
                message = "A frame from a client is not masked.";
                return false;
            }

            if (_inContinuation && frame.IsData)
            {
                message = "A data frame has been received while receiving continuation frames.";
                return false;
            }

            if (frame.IsCompressed && _compression == CompressionMethod.None)
            {
                message = "A compressed frame has been received without any agreement for it.";
                return false;
            }

            if (frame.Rsv2 == Rsv.On)
            {
                message = "The RSV2 of a frame is non-zero without any negotiation for it.";
                return false;
            }

            if (frame.Rsv3 == Rsv.On)
            {
                message = "The RSV3 of a frame is non-zero without any negotiation for it.";
                return false;
            }

            return true;
        }

        private void _Close(ushort code, string reason)
        {
            if (_readyState == WebSocketState.Closing)
            {
                return;
            }

            if (_readyState == WebSocketState.Closed)
            {
                return;
            }

            if (code == 1005)
            { // == no status
                Close(PayloadData.Empty, true, true, false);
                return;
            }

            var send = !code.IsReserved();
            Close(new PayloadData(code, reason), send, send, false);
        }

        private void Close(PayloadData payloadData, bool send, bool receive, bool received)
        {
            lock (_forState)
            {
                if (_readyState == WebSocketState.Closing)
                {
                    Console.WriteLine("The closing is already in progress.");
                    return;
                }

                if (_readyState == WebSocketState.Closed)
                {
                    Console.WriteLine("The connection has already been closed.");
                    return;
                }

                send = send && _readyState == WebSocketState.Open;
                receive = send && receive;

                _readyState = WebSocketState.Closing;
            }

            Console.WriteLine("Begin closing the connection.");

            var res = CloseHandshake(payloadData, send, receive, received);
            ReleaseResources();

            Console.WriteLine("End closing the connection.");

            _readyState = WebSocketState.Closed;

            var e = new CloseEventArgs(payloadData, res);

            try
            {
                OnClose.Emit(this, e);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void _CloseAsync(ushort code, string reason)
        {
            if (_readyState == WebSocketState.Closing)
            {
                Console.WriteLine("The closing is already in progress.");
                return;
            }

            if (_readyState == WebSocketState.Closed)
            {
                Console.WriteLine("The connection has already been closed.");
                return;
            }

            if (code == 1005)
            { // == no status
                CloseAsync(PayloadData.Empty, true, true, false);
                return;
            }

            var send = !code.IsReserved();
            CloseAsync(new PayloadData(code, reason), send, send, false);
        }

        private void CloseAsync(
          PayloadData payloadData, bool send, bool receive, bool received
        )
        {
            Action<PayloadData, bool, bool, bool> closer = Close;
            closer.BeginInvoke(
              payloadData, send, receive, received, ar => closer.EndInvoke(ar), null
            );
        }

        private bool CloseHandshake(byte[] frameAsBytes, bool receive, bool received)
        {
            var sent = frameAsBytes != null && sendBytes(frameAsBytes);

            var wait = !received && sent && receive && _receivingExited != null;
            if (wait)
                received = _receivingExited.WaitOne(_waitTime);

            var ret = sent && received;

            Console.WriteLine(
              String.Format(
                "Was clean?: {0}\n  sent: {1}\n  received: {2}", ret, sent, received
              )
            );

            return ret;
        }

        private bool CloseHandshake(PayloadData payloadData, bool send, bool receive, bool received)
        {
            var sent = false;
            if (send)
            {
                var frame = WebSocketFrame.CreateCloseFrame(payloadData, _client);
                sent = sendBytes(frame.ToArray());

                if (_client)
                    frame.Unmask();
            }

            var wait = !received && sent && receive && _receivingExited != null;
            if (wait)
                received = _receivingExited.WaitOne(_waitTime);

            var ret = sent && received;

            Console.WriteLine(
              String.Format(
                "Was clean?: {0}\n  sent: {1}\n  received: {2}", ret, sent, received
              )
            );

            return ret;
        }

        /// <summary>
        /// 建立连接
        /// </summary>
        /// <returns></returns>
        private bool _Connect()
        {
            if (_readyState == WebSocketState.Open)
            {
                var msg = "The connection has already been established.";
                Console.WriteLine(msg);

                return false;
            }

            lock (_forState)
            {
                if (_readyState == WebSocketState.Open)
                {
                    var msg = "The connection has already been established.";
                    Console.WriteLine(msg);

                    return false;
                }

                if (_readyState == WebSocketState.Closing)
                {
                    var msg = "The close process has set in.";
                    Console.WriteLine(msg);

                    msg = "An interruption has occurred while attempting to connect.";
                    Error(msg, null);

                    return false;
                }

                if (_retryCountForConnect > _maxRetryCountForConnect)
                {
                    var msg = "An opportunity for reconnecting has been lost.";
                    Console.WriteLine(msg);

                    msg = "An interruption has occurred while attempting to connect.";
                    Error(msg, null);

                    return false;
                }

                _readyState = WebSocketState.Connecting;

                try
                {
                    DoHandshake();
                }
                catch (Exception ex)
                {
                    _retryCountForConnect++;

                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.ToString());

                    var msg = "An exception has occurred while attempting to connect.";
                    Fatal(msg, ex);

                    return false;
                }

                _retryCountForConnect = 1;
                _readyState = WebSocketState.Open;

                return true;
            }
        }

        // As client
        private string CreateExtensions()
        {
            var buff = new StringBuilder(80);

            if (_compression != CompressionMethod.None)
            {
                var str = _compression.ToExtensionString(
                  "server_no_context_takeover", "client_no_context_takeover");

                buff.AppendFormat("{0}, ", str);
            }

            var len = buff.Length;
            if (len > 2)
            {
                buff.Length = len - 2;
                return buff.ToString();
            }

            return null;
        }

        // As server
        private HttpResponse CreateHandshakeFailureResponse(HttpStatusCode code)
        {
            var ret = HttpResponse.CreateCloseResponse(code);
            ret.Headers["Sec-WebSocket-Version"] = _version;

            return ret;
        }

        // As client
        private HttpRequest CreateHandshakeRequest()
        {
            var ret = HttpRequest.CreateWebSocketRequest(_uri);

            var headers = ret.Headers;
            if (!_origin.IsNullOrEmpty())
                headers["Origin"] = _origin;

            headers["Sec-WebSocket-Key"] = _base64Key;

            _protocolsRequested = _protocols != null;
            if (_protocolsRequested)
                headers["Sec-WebSocket-Protocol"] = _protocols.ToString(", ");

            _extensionsRequested = _compression != CompressionMethod.None;
            if (_extensionsRequested)
                headers["Sec-WebSocket-Extensions"] = CreateExtensions();

            headers["Sec-WebSocket-Version"] = _version;

            AuthenticationResponse authRes = null;
            if (_authChallenge != null && _credentials != null)
            {
                authRes = new AuthenticationResponse(_authChallenge, _credentials, _nonceCount);
                _nonceCount = authRes.NonceCount;
            }
            else if (_preAuth)
            {
                authRes = new AuthenticationResponse(_credentials);
            }

            if (authRes != null)
                headers["Authorization"] = authRes.ToString();

            if (_cookies.Count > 0)
                ret.SetCookies(_cookies);

            return ret;
        }

        // As server
        private HttpResponse CreateHandshakeResponse()
        {
            var ret = HttpResponse.CreateWebSocketResponse();

            var headers = ret.Headers;
            headers["Sec-WebSocket-Accept"] = CreateResponseKey(_base64Key);

            if (_protocol != null)
                headers["Sec-WebSocket-Protocol"] = _protocol;

            if (_extensions != null)
                headers["Sec-WebSocket-Extensions"] = _extensions;

            if (_cookies.Count > 0)
                ret.SetCookies(_cookies);

            return ret;
        }

        // As server
        private bool CustomCheckHandshakeRequest(
          WebSocketContext context, out string message
        )
        {
            message = null;

            if (_handshakeRequestChecker == null)
                return true;

            message = _handshakeRequestChecker(context);
            return message == null;
        }

        private MessageEventArgs DequeueFromMessageEventQueue()
        {
            lock (_forMessageEventQueue)
                return _messageEventQueue.Count > 0 ? _messageEventQueue.Dequeue() : null;
        }

        // As client
        private void DoHandshake()
        {
            SetClientStream();
            var res = SendHandshakeRequest();

            string msg;
            if (!CheckHandshakeResponse(res, out msg))
                throw new WebSocketException(CloseStatusCode.ProtocolError, msg);

            if (_protocolsRequested)
                _protocol = res.Headers["Sec-WebSocket-Protocol"];

            if (_extensionsRequested)
                ProcessSecWebSocketExtensionsServerHeader(res.Headers["Sec-WebSocket-Extensions"]);

            ProcessCookies(res.Cookies);
        }

        private void EnqueueToMessageEventQueue(MessageEventArgs e)
        {
            lock (_forMessageEventQueue)
                _messageEventQueue.Enqueue(e);
        }

        private void Error(string message, Exception exception)
        {
            try
            {
                OnError.Emit(this, new Kogel.Net.WebSocket.Extension.ErrorEventArgs(message, exception));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.ToString());
            }
        }

        private void Fatal(string message, Exception exception)
        {
            var code = exception is WebSocketException
                       ? ((WebSocketException)exception).Code
                       : CloseStatusCode.Abnormal;

            Fatal(message, (ushort)code);
        }

        private void Fatal(string message, ushort code)
        {
            var payload = new PayloadData(code, message);
            Close(payload, !code.IsReserved(), false, false);
        }

        private void Fatal(string message, CloseStatusCode code)
        {
            Fatal(message, (ushort)code);
        }

        private ClientSslConfiguration GetSslConfiguration()
        {
            if (_sslConfig == null)
                _sslConfig = new ClientSslConfiguration(_uri.DnsSafeHost);

            return _sslConfig;
        }

        private void Init()
        {
            _compression = CompressionMethod.None;
            _cookies = new CookieCollection();
            _forPing = new object();
            _forSend = new object();
            _forState = new object();
            _messageEventQueue = new Queue<MessageEventArgs>();
            _forMessageEventQueue = ((System.Collections.ICollection)_messageEventQueue).SyncRoot;
            _readyState = WebSocketState.Connecting;
        }

        private void Message()
        {
            MessageEventArgs e = null;
            lock (_forMessageEventQueue)
            {
                if (_inMessage || _messageEventQueue.Count == 0 || _readyState != WebSocketState.Open)
                    return;

                _inMessage = true;
                e = _messageEventQueue.Dequeue();
            }

            _message(e);
        }

        private void Messagec(MessageEventArgs e)
        {
            do
            {
                try
                {
                    OnMessage.Emit(this, e);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    Error("An error has occurred during an OnMessage event.", ex);
                }

                lock (_forMessageEventQueue)
                {
                    if (_messageEventQueue.Count == 0 || _readyState != WebSocketState.Open)
                    {
                        _inMessage = false;
                        break;
                    }

                    e = _messageEventQueue.Dequeue();
                }
            }
            while (true);
        }

        private void Messages(MessageEventArgs e)
        {
            try
            {
                OnMessage.Emit(this, e);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Error("An error has occurred during an OnMessage event.", ex);
            }

            lock (_forMessageEventQueue)
            {
                if (_messageEventQueue.Count == 0 || _readyState != WebSocketState.Open)
                {
                    _inMessage = false;
                    return;
                }

                e = _messageEventQueue.Dequeue();
            }

            ThreadPool.QueueUserWorkItem(state => Messages(e));
        }

        private void Open()
        {
            _inMessage = true;
            StartReceiving();
            try
            {
                OnOpen.Emit(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Error("An error has occurred during the OnOpen event.", ex);
            }

            MessageEventArgs e = null;
            lock (_forMessageEventQueue)
            {
                if (_messageEventQueue.Count == 0 || _readyState != WebSocketState.Open)
                {
                    _inMessage = false;
                    return;
                }

                e = _messageEventQueue.Dequeue();
            }

            _message.BeginInvoke(e, ar => _message.EndInvoke(ar), null);
        }

        private bool Ping(byte[] data)
        {
            if (_readyState != WebSocketState.Open)
                return false;

            var pongReceived = _pongReceived;
            if (pongReceived == null)
                return false;

            lock (_forPing)
            {
                try
                {
                    pongReceived.Reset();
                    if (!Send(Fin.Final, Opcode.Ping, data, false))
                        return false;

                    return pongReceived.WaitOne(_waitTime);
                }
                catch (ObjectDisposedException)
                {
                    return false;
                }
            }
        }

        private bool ProcessCloseFrame(WebSocketFrame frame)
        {
            var payload = frame.PayloadData;
            Close(payload, !payload.HasReservedCode, false, true);

            return false;
        }

        // As client
        private void ProcessCookies(CookieCollection cookies)
        {
            if (cookies.Count == 0)
                return;

            _cookies.SetOrRemove(cookies);
        }

        private bool ProcessDataFrame(WebSocketFrame frame)
        {
            EnqueueToMessageEventQueue(
              frame.IsCompressed
              ? new MessageEventArgs(
                  frame.Opcode, frame.PayloadData.ApplicationData.Decompress(_compression))
              : new MessageEventArgs(frame));

            return true;
        }

        private bool ProcessFragmentFrame(WebSocketFrame frame)
        {
            if (!_inContinuation)
            {
                // Must process first fragment.
                if (frame.IsContinuation)
                    return true;

                _fragmentsOpcode = frame.Opcode;
                _fragmentsCompressed = frame.IsCompressed;
                _fragmentsBuffer = new MemoryStream();
                _inContinuation = true;
            }

            _fragmentsBuffer.WriteBytes(frame.PayloadData.ApplicationData, 1024);
            if (frame.IsFinal)
            {
                using (_fragmentsBuffer)
                {
                    var data = _fragmentsCompressed
                               ? _fragmentsBuffer.DecompressToArray(_compression)
                               : _fragmentsBuffer.ToArray();

                    EnqueueToMessageEventQueue(new MessageEventArgs(_fragmentsOpcode, data));
                }

                _fragmentsBuffer = null;
                _inContinuation = false;
            }

            return true;
        }

        private bool ProcessPingFrame(WebSocketFrame frame)
        {
            Console.WriteLine("A ping was received.");

            var pong = WebSocketFrame.CreatePongFrame(frame.PayloadData, _client);

            lock (_forState)
            {
                if (_readyState != WebSocketState.Open)
                {
                    Console.WriteLine("The connection is closing.");
                    return true;
                }

                if (!sendBytes(pong.ToArray()))
                    return false;
            }

            Console.WriteLine("A pong to this ping has been sent.");

            if (_emitOnPing)
            {
                if (_client)
                    pong.Unmask();

                EnqueueToMessageEventQueue(new MessageEventArgs(frame));
            }

            return true;
        }

        private bool ProcessPongFrame(WebSocketFrame frame)
        {
            Console.WriteLine("A pong was received.");

            try
            {
                _pongReceived.Set();
            }
            catch (NullReferenceException ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.ToString());

                return false;
            }
            catch (ObjectDisposedException ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.ToString());

                return false;
            }

            Console.WriteLine("It has been signaled.");

            return true;
        }

        private bool ProcessReceivedFrame(WebSocketFrame frame)
        {
            string msg;
            if (!CheckReceivedFrame(frame, out msg))
                throw new WebSocketException(CloseStatusCode.ProtocolError, msg);

            frame.Unmask();
            return frame.IsFragment
                   ? ProcessFragmentFrame(frame)
                   : frame.IsData
                     ? ProcessDataFrame(frame)
                     : frame.IsPing
                       ? ProcessPingFrame(frame)
                       : frame.IsPong
                         ? ProcessPongFrame(frame)
                         : frame.IsClose
                           ? ProcessCloseFrame(frame)
                           : ProcessUnsupportedFrame(frame);
        }

        // As server
        private void ProcessSecWebSocketExtensionsClientHeader(string value)
        {
            if (value == null)
                return;

            var buff = new StringBuilder(80);
            var comp = false;

            foreach (var elm in value.SplitHeaderValue(','))
            {
                var extension = elm.Trim();
                if (extension.Length == 0)
                    continue;

                if (!comp)
                {
                    if (extension.IsCompressionExtension(CompressionMethod.Deflate))
                    {
                        _compression = CompressionMethod.Deflate;

                        buff.AppendFormat(
                          "{0}, ",
                          _compression.ToExtensionString(
                            "client_no_context_takeover", "server_no_context_takeover"
                          )
                        );

                        comp = true;
                    }
                }
            }

            var len = buff.Length;
            if (len <= 2)
                return;

            buff.Length = len - 2;
            _extensions = buff.ToString();
        }

        // As client
        private void ProcessSecWebSocketExtensionsServerHeader(string value)
        {
            if (value == null)
            {
                _compression = CompressionMethod.None;
                return;
            }

            _extensions = value;
        }

        // As server
        private void ProcessSecWebSocketProtocolClientHeader(
          IEnumerable<string> values
        )
        {
            if (values.Contains(val => val == _protocol))
                return;

            _protocol = null;
        }

        private bool ProcessUnsupportedFrame(WebSocketFrame frame)
        {
            Console.WriteLine("An unsupported frame:" + frame.PrintToString(false));
            Fatal("There is no way to handle it.", CloseStatusCode.PolicyViolation);

            return false;
        }

        // As server
        private void RefuseHandshake(CloseStatusCode code, string reason)
        {
            _readyState = WebSocketState.Closing;

            var res = CreateHandshakeFailureResponse(HttpStatusCode.BadRequest);
            sendHttpResponse(res);

            ReleaseServerResources();

            _readyState = WebSocketState.Closed;

            var e = new CloseEventArgs((ushort)code, reason, false);

            try
            {
                OnClose.Emit(this, e);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.ToString());
            }
        }

        // As client
        private void ReleaseClientResources()
        {
            if (_stream != null)
            {
                _stream.Dispose();
                _stream = null;
            }

            if (_tcpClient != null)
            {
                _tcpClient.Close();
                _tcpClient = null;
            }
        }

        private void ReleaseCommonResources()
        {
            if (_fragmentsBuffer != null)
            {
                _fragmentsBuffer.Dispose();
                _fragmentsBuffer = null;
                _inContinuation = false;
            }

            if (_pongReceived != null)
            {
                _pongReceived.Close();
                _pongReceived = null;
            }

            if (_receivingExited != null)
            {
                _receivingExited.Close();
                _receivingExited = null;
            }
        }

        private void ReleaseResources()
        {
            if (_client)
                ReleaseClientResources();
            else
                ReleaseServerResources();

            ReleaseCommonResources();
        }

        // As server
        private void ReleaseServerResources()
        {
            if (_closeContext == null)
                return;

            _closeContext();
            _closeContext = null;
            _stream = null;
            _context = null;
        }

        private bool Send(Opcode opcode, Stream stream)
        {
            lock (_forSend)
            {
                var src = stream;
                var compressed = false;
                var sent = false;
                try
                {
                    if (_compression != CompressionMethod.None)
                    {
                        stream = stream.Compress(_compression);
                        compressed = true;
                    }

                    sent = Send(opcode, stream, compressed);
                    if (!sent)
                        Error("A send has been interrupted.", null);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    Error("An error has occurred during a send.", ex);
                }
                finally
                {
                    if (compressed)
                        stream.Dispose();

                    src.Dispose();
                }

                return sent;
            }
        }

        private bool Send(Opcode opcode, Stream stream, bool compressed)
        {
            var len = stream.Length;
            if (len == 0)
                return Send(Fin.Final, opcode, EmptyBytes, false);

            var quo = len / FragmentLength;
            var rem = (int)(len % FragmentLength);

            byte[] buff = null;
            if (quo == 0)
            {
                buff = new byte[rem];
                return stream.Read(buff, 0, rem) == rem
                       && Send(Fin.Final, opcode, buff, compressed);
            }

            if (quo == 1 && rem == 0)
            {
                buff = new byte[FragmentLength];
                return stream.Read(buff, 0, FragmentLength) == FragmentLength
                       && Send(Fin.Final, opcode, buff, compressed);
            }

            /* Send fragments */

            // Begin
            buff = new byte[FragmentLength];
            var sent = stream.Read(buff, 0, FragmentLength) == FragmentLength
                       && Send(Fin.More, opcode, buff, compressed);

            if (!sent)
                return false;

            var n = rem == 0 ? quo - 2 : quo - 1;
            for (long i = 0; i < n; i++)
            {
                sent = stream.Read(buff, 0, FragmentLength) == FragmentLength
                       && Send(Fin.More, Opcode.Cont, buff, false);

                if (!sent)
                    return false;
            }

            // End
            if (rem == 0)
                rem = FragmentLength;
            else
                buff = new byte[rem];

            return stream.Read(buff, 0, rem) == rem
                   && Send(Fin.Final, Opcode.Cont, buff, false);
        }

        private bool Send(Fin fin, Opcode opcode, byte[] data, bool compressed)
        {
            lock (_forState)
            {
                if (_readyState != WebSocketState.Open)
                {
                    Console.WriteLine("The connection is closing.");
                    return false;
                }

                var frame = new WebSocketFrame(fin, opcode, data, compressed, _client);
                return sendBytes(frame.ToArray());
            }
        }

        private void SendAsync(Opcode opcode, Stream stream, Action<bool> completed)
        {
            Func<Opcode, Stream, bool> sender = Send;
            sender.BeginInvoke(
              opcode,
              stream,
              ar =>
              {
                  try
                  {
                      var sent = sender.EndInvoke(ar);
                      if (completed != null)
                          completed(sent);
                  }
                  catch (Exception ex)
                  {
                      Console.WriteLine(ex.ToString());
                      Error(
                  "An error has occurred during the callback for an async send.",
                  ex
                );
                  }
              },
              null
            );
        }

        private bool sendBytes(byte[] bytes)
        {
            try
            {
                _stream.Write(bytes, 0, bytes.Length);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.ToString());

                return false;
            }

            return true;
        }

        // As client
        private HttpResponse SendHandshakeRequest()
        {
            var req = CreateHandshakeRequest();
            var res = SendHttpRequest(req, 90000);
            if (res.IsUnauthorized)
            {
                var chal = res.Headers["WWW-Authenticate"];
                Console.WriteLine(String.Format("Received an authentication requirement for '{0}'.", chal));
                if (chal.IsNullOrEmpty())
                {
                    Console.WriteLine("No authentication challenge is specified.");
                    return res;
                }

                _authChallenge = AuthenticationChallenge.Parse(chal);
                if (_authChallenge == null)
                {
                    Console.WriteLine("An invalid authentication challenge is specified.");
                    return res;
                }

                if (_credentials != null &&
                    (!_preAuth || _authChallenge.Scheme == AuthenticationSchemes.Digest))
                {
                    if (res.HasConnectionClose)
                    {
                        ReleaseClientResources();
                        SetClientStream();
                    }

                    var authRes = new AuthenticationResponse(_authChallenge, _credentials, _nonceCount);
                    _nonceCount = authRes.NonceCount;
                    req.Headers["Authorization"] = authRes.ToString();
                    res = SendHttpRequest(req, 15000);
                }
            }

            if (res.IsRedirect)
            {
                var url = res.Headers["Location"];
                Console.WriteLine(String.Format("Received a redirection to '{0}'.", url));
                if (_enableRedirection)
                {
                    if (url.IsNullOrEmpty())
                    {
                        Console.WriteLine("No url to redirect is located.");
                        return res;
                    }

                    Uri uri;
                    string msg;
                    if (!url.TryCreateWebSocketUri(out uri, out msg))
                    {
                        Console.WriteLine("An invalid url to redirect is located: " + msg);
                        return res;
                    }

                    ReleaseClientResources();

                    _uri = uri;
                    _secure = uri.Scheme == "wss";

                    SetClientStream();
                    return SendHandshakeRequest();
                }
            }

            return res;
        }

        // As client
        private HttpResponse SendHttpRequest(HttpRequest request, int millisecondsTimeout)
        {
            Console.WriteLine("A request to the server:\n" + request.ToString());
            var res = request.GetResponse(_stream, millisecondsTimeout);
            Console.WriteLine("A response to this request:\n" + res.ToString());

            return res;
        }

        // As server
        private bool sendHttpResponse(HttpResponse response)
        {
            Console.WriteLine(
              String.Format(
                "A response to {0}:\n{1}", _context.UserEndPoint, response
              )
            );

            return sendBytes(response.ToByteArray());
        }

        // As client
        private void SendProxyConnectRequest()
        {
            var req = HttpRequest.CreateConnectRequest(_uri);
            var res = SendHttpRequest(req, 90000);
            if (res.IsProxyAuthenticationRequired)
            {
                var chal = res.Headers["Proxy-Authenticate"];
                Console.WriteLine(
                  String.Format("Received a proxy authentication requirement for '{0}'.", chal));

                if (chal.IsNullOrEmpty())
                    throw new WebSocketException("No proxy authentication challenge is specified.");

                var authChal = AuthenticationChallenge.Parse(chal);
                if (authChal == null)
                    throw new WebSocketException("An invalid proxy authentication challenge is specified.");

                if (_proxyCredentials != null)
                {
                    if (res.HasConnectionClose)
                    {
                        ReleaseClientResources();
                        _tcpClient = new TcpClient(_proxyUri.DnsSafeHost, _proxyUri.Port);
                        _stream = _tcpClient.GetStream();
                    }

                    var authRes = new AuthenticationResponse(authChal, _proxyCredentials, 0);
                    req.Headers["Proxy-Authorization"] = authRes.ToString();
                    res = SendHttpRequest(req, 15000);
                }

                if (res.IsProxyAuthenticationRequired)
                    throw new WebSocketException("A proxy authentication is required.");
            }

            if (res.StatusCode[0] != '2')
                throw new WebSocketException(
                  "The proxy has failed a connection to the requested host and port.");
        }

        // As client
        private void SetClientStream()
        {
            if (_proxyUri != null)
            {
                _tcpClient = new TcpClient(_proxyUri.DnsSafeHost, _proxyUri.Port);
                _stream = _tcpClient.GetStream();
                SendProxyConnectRequest();
            }
            else
            {
                _tcpClient = new TcpClient(_uri.DnsSafeHost, _uri.Port);
                _stream = _tcpClient.GetStream();
            }

            if (_secure)
            {
                var conf = GetSslConfiguration();
                var host = conf.TargetHost;
                if (host != _uri.DnsSafeHost)
                    throw new WebSocketException(
                      CloseStatusCode.TlsHandshakeFailure, "An invalid host name is specified.");

                try
                {
                    var sslStream = new SslStream(
                      _stream,
                      false,
                      conf.ServerCertificateValidationCallback,
                      conf.ClientCertificateSelectionCallback);

                    sslStream.AuthenticateAsClient(
                      host,
                      conf.ClientCertificates,
                      conf.EnabledSslProtocols,
                      conf.CheckCertificateRevocation);

                    _stream = sslStream;
                }
                catch (Exception ex)
                {
                    throw new WebSocketException(CloseStatusCode.TlsHandshakeFailure, ex);
                }
            }
        }

        private void StartReceiving()
        {
            if (_messageEventQueue.Count > 0)
                _messageEventQueue.Clear();

            _pongReceived = new ManualResetEvent(false);
            _receivingExited = new ManualResetEvent(false);

            Action receive = null;
            receive =
              () =>
                WebSocketFrame.ReadFrameAsync(
                  _stream,
                  false,
                  frame =>
                  {
                      if (!ProcessReceivedFrame(frame) || _readyState == WebSocketState.Closed)
                      {
                          var exited = _receivingExited;
                          if (exited != null)
                              exited.Set();

                          return;
                      }

                      // Receive next asap because the Ping or Close needs a response to it.
                      receive();

                      if (_inMessage || !HasMessage || _readyState != WebSocketState.Open)
                          return;

                      Message();
                  },
                  ex =>
                  {
                      Console.WriteLine(ex.ToString());
                      Fatal("An exception has occurred while receiving.", ex);
                  }
                );

            receive();
        }

        // As client
        private bool validateSecWebSocketAcceptHeader(string value)
        {
            return value != null && value == CreateResponseKey(_base64Key);
        }

        // As client
        private bool validateSecWebSocketExtensionsServerHeader(string value)
        {
            if (value == null)
                return true;

            if (value.Length == 0)
                return false;

            if (!_extensionsRequested)
                return false;

            var comp = _compression != CompressionMethod.None;
            foreach (var e in value.SplitHeaderValue(','))
            {
                var ext = e.Trim();
                if (comp && ext.IsCompressionExtension(_compression))
                {
                    if (!ext.Contains("server_no_context_takeover"))
                    {
                        Console.WriteLine("The server hasn't sent back 'server_no_context_takeover'.");
                        return false;
                    }

                    if (!ext.Contains("client_no_context_takeover"))
                        Console.WriteLine("The server hasn't sent back 'client_no_context_takeover'.");

                    var method = _compression.ToExtensionString();
                    var invalid =
                      ext.SplitHeaderValue(';').Contains(
                        t =>
                        {
                            t = t.Trim();
                            return t != method
                         && t != "server_no_context_takeover"
                         && t != "client_no_context_takeover";
                        }
                      );

                    if (invalid)
                        return false;
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        // As client
        private bool validateSecWebSocketProtocolServerHeader(string value)
        {
            if (value == null)
                return !_protocolsRequested;

            if (value.Length == 0)
                return false;

            return _protocolsRequested && _protocols.Contains(p => p == value);
        }

        // As client
        private bool validateSecWebSocketVersionServerHeader(string value)
        {
            return value == null || value == _version;
        }





        // As server
        internal void Close(HttpResponse response)
        {
            _readyState = WebSocketState.Closing;

            sendHttpResponse(response);
            ReleaseServerResources();

            _readyState = WebSocketState.Closed;
        }

        // As server
        internal void Close(HttpStatusCode code)
        {
            Close(CreateHandshakeFailureResponse(code));
        }

        // As server
        internal void Close(PayloadData payloadData, byte[] frameAsBytes)
        {
            lock (_forState)
            {
                if (_readyState == WebSocketState.Closing)
                {
                    Console.WriteLine("The closing is already in progress.");
                    return;
                }

                if (_readyState == WebSocketState.Closed)
                {
                    Console.WriteLine("The connection has already been closed.");
                    return;
                }

                _readyState = WebSocketState.Closing;
            }

            Console.WriteLine("Begin closing the connection.");

            var sent = frameAsBytes != null && sendBytes(frameAsBytes);
            var received = sent && _receivingExited != null
                           ? _receivingExited.WaitOne(_waitTime)
                           : false;

            var res = sent && received;

            Console.WriteLine(
              String.Format(
                "Was clean?: {0}\n  sent: {1}\n  received: {2}", res, sent, received
              )
            );

            ReleaseServerResources();
            ReleaseCommonResources();

            Console.WriteLine("End closing the connection.");

            _readyState = WebSocketState.Closed;

            var e = new CloseEventArgs(payloadData, res);

            try
            {
                OnClose.Emit(this, e);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.ToString());
            }
        }

        // As client
        internal static string CreateBase64Key()
        {
            var src = new byte[16];
            RandomNumber.GetBytes(src);

            return Convert.ToBase64String(src);
        }

        internal static string CreateResponseKey(string base64Key)
        {
            var buff = new StringBuilder(base64Key, 64);
            buff.Append(_guid);
            SHA1 sha1 = new SHA1CryptoServiceProvider();
            var src = sha1.ComputeHash(buff.ToString().GetUTF8EncodedBytes());

            return Convert.ToBase64String(src);
        }

        // As server
        internal void InternalAccept()
        {
            try
            {
                if (!AcceptHandshake())
                    return;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.ToString());

                var msg = "An exception has occurred while attempting to accept.";
                Fatal(msg, ex);

                return;
            }

            _readyState = WebSocketState.Open;

            Open();
        }

        // As server
        internal bool Ping(byte[] frameAsBytes, TimeSpan timeout)
        {
            if (_readyState != WebSocketState.Open)
                return false;

            var pongReceived = _pongReceived;
            if (pongReceived == null)
                return false;

            lock (_forPing)
            {
                try
                {
                    pongReceived.Reset();

                    lock (_forState)
                    {
                        if (_readyState != WebSocketState.Open)
                            return false;

                        if (!sendBytes(frameAsBytes))
                            return false;
                    }

                    return pongReceived.WaitOne(timeout);
                }
                catch (ObjectDisposedException)
                {
                    return false;
                }
            }
        }

        // As server
        internal void Send(
          Opcode opcode, byte[] data, Dictionary<CompressionMethod, byte[]> cache
        )
        {
            lock (_forSend)
            {
                lock (_forState)
                {
                    if (_readyState != WebSocketState.Open)
                    {
                        Console.WriteLine("The connection is closing.");
                        return;
                    }

                    byte[] found;
                    if (!cache.TryGetValue(_compression, out found))
                    {
                        found = new WebSocketFrame(
                                  Fin.Final,
                                  opcode,
                                  data.Compress(_compression),
                                  _compression != CompressionMethod.None,
                                  false
                                )
                                .ToArray();

                        cache.Add(_compression, found);
                    }

                    sendBytes(found);
                }
            }
        }

        // As server
        internal void Send(
          Opcode opcode, Stream stream, Dictionary<CompressionMethod, Stream> cache
        )
        {
            lock (_forSend)
            {
                Stream found;
                if (!cache.TryGetValue(_compression, out found))
                {
                    found = stream.Compress(_compression);
                    cache.Add(_compression, found);
                }
                else
                {
                    found.Position = 0;
                }

                Send(opcode, found, _compression != CompressionMethod.None);
            }
        }




        /// <summary>
        /// 
        /// </summary>
        public void _Accept()
        {
            if (_client)
            {
                var msg = "This instance is a client.";
                throw new InvalidOperationException(msg);
            }

            if (_readyState == WebSocketState.Closing)
            {
                var msg = "The close process is in progress.";
                throw new InvalidOperationException(msg);
            }

            if (_readyState == WebSocketState.Closed)
            {
                var msg = "The connection has already been closed.";
                throw new InvalidOperationException(msg);
            }

            if (Accept())
                Open();
        }

        /// <summary>
        /// Accepts the handshake request asynchronously.
        /// </summary>
        /// <remarks>
        ///   <para>
        ///   This method does not wait for the accept process to be complete.
        ///   </para>
        ///   <para>
        ///   This method does nothing if the handshake request has already been
        ///   accepted.
        ///   </para>
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        ///   <para>
        ///   This instance is a client.
        ///   </para>
        ///   <para>
        ///   -or-
        ///   </para>
        ///   <para>
        ///   The close process is in progress.
        ///   </para>
        ///   <para>
        ///   -or-
        ///   </para>
        ///   <para>
        ///   The connection has already been closed.
        ///   </para>
        /// </exception>
        public void AcceptAsync()
        {
            if (_client)
            {
                var msg = "This instance is a client.";
                throw new InvalidOperationException(msg);
            }

            if (_readyState == WebSocketState.Closing)
            {
                var msg = "The close process is in progress.";
                throw new InvalidOperationException(msg);
            }

            if (_readyState == WebSocketState.Closed)
            {
                var msg = "The connection has already been closed.";
                throw new InvalidOperationException(msg);
            }

            Func<bool> acceptor = Accept;
            acceptor.BeginInvoke(
              ar =>
              {
                  if (acceptor.EndInvoke(ar))
                      Open();
              },
              null
            );
        }

        /// <summary>
        /// 
        /// </summary>
        public void Close()
        {
            _Close(1005, String.Empty);
        }

        /// <summary>
        /// 关闭与指定代码的连接。
        /// </summary>
        /// <param name="code"></param>
        public void Close(ushort code)
        {
            if (!code.IsCloseStatusCode())
            {
                var msg = "Less than 1000 or greater than 4999.";
                throw new ArgumentOutOfRangeException("code", msg);
            }

            if (_client && code == 1011)
            {
                var msg = "1011 cannot be used.";
                throw new ArgumentException(msg, "code");
            }

            if (!_client && code == 1010)
            {
                var msg = "1010 cannot be used.";
                throw new ArgumentException(msg, "code");
            }

            _Close(code, String.Empty);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="code"></param>
        public void Close(CloseStatusCode code)
        {
            if (_client && code == CloseStatusCode.ServerError)
            {
                var msg = "ServerError cannot be used.";
                throw new ArgumentException(msg, "code");
            }

            if (!_client && code == CloseStatusCode.MandatoryExtension)
            {
                var msg = "MandatoryExtension cannot be used.";
                throw new ArgumentException(msg, "code");
            }

            Close((ushort)code, String.Empty);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="code"></param>
        /// <param name="reason"></param>
        public void Close(ushort code, string reason)
        {
            if (!code.IsCloseStatusCode())
            {
                var msg = "Less than 1000 or greater than 4999.";
                throw new ArgumentOutOfRangeException("code", msg);
            }

            if (_client && code == 1011)
            {
                var msg = "1011 cannot be used.";
                throw new ArgumentException(msg, "code");
            }

            if (!_client && code == 1010)
            {
                var msg = "1010 cannot be used.";
                throw new ArgumentException(msg, "code");
            }

            if (reason.IsNullOrEmpty())
            {
                _Close(code, String.Empty);
                return;
            }

            if (code == 1005)
            {
                var msg = "1005 cannot be used.";
                throw new ArgumentException(msg, "code");
            }

            byte[] bytes;
            if (!reason.TryGetUTF8EncodedBytes(out bytes))
            {
                var msg = "It could not be UTF-8-encoded.";
                throw new ArgumentException(msg, "reason");
            }

            if (bytes.Length > 123)
            {
                var msg = "Its size is greater than 123 bytes.";
                throw new ArgumentOutOfRangeException("reason", msg);
            }

            _Close(code, reason);
        }

        /// <summary>
        /// 使用指定的代码和原因关闭连接。
        /// </summary>
        /// <param name="code"></param>
        /// <param name="reason">原因</param>
        public void Close(CloseStatusCode code, string reason)
        {
            if (_client && code == CloseStatusCode.ServerError)
            {
                var msg = "ServerError cannot be used.";
                throw new ArgumentException(msg, "code");
            }

            if (!_client && code == CloseStatusCode.MandatoryExtension)
            {
                var msg = "MandatoryExtension cannot be used.";
                throw new ArgumentException(msg, "code");
            }

            if (reason.IsNullOrEmpty())
            {
                _Close((ushort)code, String.Empty);
                return;
            }

            if (code == CloseStatusCode.NoStatus)
            {
                var msg = "NoStatus cannot be used.";
                throw new ArgumentException(msg, "code");
            }

            byte[] bytes;
            if (!reason.TryGetUTF8EncodedBytes(out bytes))
            {
                var msg = "It could not be UTF-8-encoded.";
                throw new ArgumentException(msg, "reason");
            }

            if (bytes.Length > 123)
            {
                var msg = "Its size is greater than 123 bytes.";
                throw new ArgumentOutOfRangeException("reason", msg);
            }

            _Close((ushort)code, reason);
        }

        /// <summary>
        /// 关闭连接异步
        /// </summary>
        public void CloseAsync()
        {
            _CloseAsync(1005, String.Empty);
        }

        /// <summary>
        /// 关闭指定代码连接
        /// </summary>
        /// <param name="code"></param>
        public void CloseAsync(ushort code)
        {
            if (!code.IsCloseStatusCode())
            {
                var msg = "Less than 1000 or greater than 4999.";
                throw new ArgumentOutOfRangeException("code", msg);
            }

            if (_client && code == 1011)
            {
                var msg = "1011 cannot be used.";
                throw new ArgumentException(msg, "code");
            }

            if (!_client && code == 1010)
            {
                var msg = "1010 cannot be used.";
                throw new ArgumentException(msg, "code");
            }

            _CloseAsync(code, String.Empty);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="code"></param>
        public void CloseAsync(CloseStatusCode code)
        {
            if (_client && code == CloseStatusCode.ServerError)
            {
                var msg = "ServerError cannot be used.";
                throw new ArgumentException(msg, "code");
            }

            if (!_client && code == CloseStatusCode.MandatoryExtension)
            {
                var msg = "MandatoryExtension cannot be used.";
                throw new ArgumentException(msg, "code");
            }

            _CloseAsync((ushort)code, String.Empty);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="code"></param>
        /// <param name="reason"></param>
        public void CloseAsync(ushort code, string reason)
        {
            if (!code.IsCloseStatusCode())
            {
                var msg = "Less than 1000 or greater than 4999.";
                throw new ArgumentOutOfRangeException("code", msg);
            }

            if (_client && code == 1011)
            {
                var msg = "1011 cannot be used.";
                throw new ArgumentException(msg, "code");
            }

            if (!_client && code == 1010)
            {
                var msg = "1010 cannot be used.";
                throw new ArgumentException(msg, "code");
            }

            if (reason.IsNullOrEmpty())
            {
                _CloseAsync(code, String.Empty);
                return;
            }

            if (code == 1005)
            {
                var msg = "1005 cannot be used.";
                throw new ArgumentException(msg, "code");
            }

            byte[] bytes;
            if (!reason.TryGetUTF8EncodedBytes(out bytes))
            {
                var msg = "It could not be UTF-8-encoded.";
                throw new ArgumentException(msg, "reason");
            }

            if (bytes.Length > 123)
            {
                var msg = "Its size is greater than 123 bytes.";
                throw new ArgumentOutOfRangeException("reason", msg);
            }

            _CloseAsync(code, reason);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="code"></param>
        /// <param name="reason"></param>
        public void CloseAsync(CloseStatusCode code, string reason)
        {
            if (_client && code == CloseStatusCode.ServerError)
            {
                var msg = "ServerError cannot be used.";
                throw new ArgumentException(msg, "code");
            }

            if (!_client && code == CloseStatusCode.MandatoryExtension)
            {
                var msg = "MandatoryExtension cannot be used.";
                throw new ArgumentException(msg, "code");
            }

            if (reason.IsNullOrEmpty())
            {
                _CloseAsync((ushort)code, String.Empty);
                return;
            }

            if (code == CloseStatusCode.NoStatus)
            {
                var msg = "NoStatus cannot be used.";
                throw new ArgumentException(msg, "code");
            }

            byte[] bytes;
            if (!reason.TryGetUTF8EncodedBytes(out bytes))
            {
                var msg = "It could not be UTF-8-encoded.";
                throw new ArgumentException(msg, "reason");
            }

            if (bytes.Length > 123)
            {
                var msg = "Its size is greater than 123 bytes.";
                throw new ArgumentOutOfRangeException("reason", msg);
            }

            _CloseAsync((ushort)code, reason);
        }

        /// <summary>
        /// 如果连接已经建立，则此方法不执行任何操作。
        /// </summary>
        public void Connect()
        {
            if (!_client)
            {
                var msg = "This instance is not a client.";
                throw new InvalidOperationException(msg);
            }

            if (_readyState == WebSocketState.Closing)
            {
                var msg = "The close process is in progress.";
                throw new InvalidOperationException(msg);
            }

            if (_retryCountForConnect > _maxRetryCountForConnect)
            {
                var msg = "A series of reconnecting has failed.";
                throw new InvalidOperationException(msg);
            }

            if (_Connect())
                Open();
        }

        /// <summary>
        /// 异步建立连接
        /// </summary>
        public void ConnectAsync()
        {
            if (!_client)
            {
                var msg = "This instance is not a client.";
                throw new InvalidOperationException(msg);
            }

            if (_readyState == WebSocketState.Closing)
            {
                var msg = "The close process is in progress.";
                throw new InvalidOperationException(msg);
            }

            if (_retryCountForConnect > _maxRetryCountForConnect)
            {
                var msg = "A series of reconnecting has failed.";
                throw new InvalidOperationException(msg);
            }

            Func<bool> connector = _Connect;
            connector.BeginInvoke(
              ar =>
              {
                  if (connector.EndInvoke(ar))
                      Open();
              },
              null
            );
        }

        /// <summary>
        /// 使用 WebSocket 连接发送 ping
        /// </summary>
        /// <returns></returns>
        public bool Ping()
        {
            return Ping(EmptyBytes);
        }

        /// <summary>
        /// 使用 WebSocket 连接发送 ping（指定mssage）
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public bool Ping(string message)
        {
            if (message.IsNullOrEmpty())
                return Ping(EmptyBytes);

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

            return Ping(bytes);
        }

        /// <summary>
        /// 使用 WebSocket 连接发送指定的数据。
        /// </summary>
        /// <param name="data"></param>
        public void Send(byte[] data)
        {
            if (_readyState != WebSocketState.Open)
            {
                var msg = "The current state of the connection is not Open.";
                throw new InvalidOperationException(msg);
            }

            if (data == null)
                throw new ArgumentNullException("data");

            Send(Opcode.Binary, new MemoryStream(data));
        }

        /// <summary>
        /// 使用 WebSocket 连接发送指定的文件。
        /// </summary>
        /// <param name="fileInfo"></param>
        public void Send(FileInfo fileInfo)
        {
            if (_readyState != WebSocketState.Open)
            {
                var msg = "The current state of the connection is not Open.";
                throw new InvalidOperationException(msg);
            }

            if (fileInfo == null)
                throw new ArgumentNullException("fileInfo");

            if (!fileInfo.Exists)
            {
                var msg = "The file does not exist.";
                throw new ArgumentException(msg, "fileInfo");
            }

            FileStream stream;
            if (!fileInfo.TryOpenRead(out stream))
            {
                var msg = "The file could not be opened.";
                throw new ArgumentException(msg, "fileInfo");
            }

            Send(Opcode.Binary, stream);
        }

        /// <summary>
        /// 使用 WebSocket 连接发送指定的字符串
        /// </summary>
        /// <param name="data"></param>
        public void Send(string data)
        {
            if (_readyState != WebSocketState.Open)
            {
                var msg = "The current state of the connection is not Open.";
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

            Send(Opcode.Text, new MemoryStream(bytes));
        }

        /// <summary>
        /// 使用 WebSocket 连接从指定的流发送数据。
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="length"></param>
        public void Send(Stream stream, int length)
        {
            if (_readyState != WebSocketState.Open)
            {
                var msg = "The current state of the connection is not Open.";
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

            Send(Opcode.Binary, new MemoryStream(bytes));
        }

        /// <summary>
        /// 使用 WebSocket 连接异步发送指定数据。
        /// </summary>
        /// <param name="data"></param>
        /// <param name="completed"></param>
        public void SendAsync(byte[] data, Action<bool> completed)
        {
            if (_readyState != WebSocketState.Open)
            {
                var msg = "The current state of the connection is not Open.";
                throw new InvalidOperationException(msg);
            }

            if (data == null)
                throw new ArgumentNullException("data");

            SendAsync(Opcode.Binary, new MemoryStream(data), completed);
        }

        /// <summary>
        ///  使用 WebSocket 连接异步发送指定文件。
        /// </summary>
        /// <param name="fileInfo"></param>
        /// <param name="completed"></param>
        public void SendAsync(FileInfo fileInfo, Action<bool> completed)
        {
            if (_readyState != WebSocketState.Open)
            {
                var msg = "The current state of the connection is not Open.";
                throw new InvalidOperationException(msg);
            }

            if (fileInfo == null)
                throw new ArgumentNullException("fileInfo");

            if (!fileInfo.Exists)
            {
                var msg = "The file does not exist.";
                throw new ArgumentException(msg, "fileInfo");
            }

            FileStream stream;
            if (!fileInfo.TryOpenRead(out stream))
            {
                var msg = "The file could not be opened.";
                throw new ArgumentException(msg, "fileInfo");
            }

            SendAsync(Opcode.Binary, stream, completed);
        }

        /// <summary>
        /// 使用 WebSocket 连接异步发送指定字符串。
        /// </summary>
        /// <param name="data"></param>
        /// <param name="completed"></param>
        public void SendAsync(string data, Action<bool> completed)
        {
            if (_readyState != WebSocketState.Open)
            {
                var msg = "The current state of the connection is not Open.";
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

            SendAsync(Opcode.Text, new MemoryStream(bytes), completed);
        }

        /// <summary>
        /// 使用 WebSocket 连接从指定的流发送数据。
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="length"></param>
        /// <param name="completed"></param>
        public void SendAsync(Stream stream, int length, Action<bool> completed)
        {
            if (_readyState != WebSocketState.Open)
            {
                var msg = "The current state of the connection is not Open.";
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

            SendAsync(Opcode.Binary, new MemoryStream(bytes), completed);
        }

        /// <summary>
        /// 设置与握手请求一起发送的 HTTP cookie。
        /// </summary>
        /// <remarks>
        /// This method does nothing if the connection has already been
        /// established or it is closing.
        /// </remarks>
        /// <param name="cookie">
        /// A <see cref="Cookie"/> that represents the cookie to send.
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// This instance is not a client.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="cookie"/> is <see langword="null"/>.
        /// </exception>
        public void SetCookie(Cookie cookie)
        {
            string msg = null;

            if (!_client)
            {
                msg = "This instance is not a client.";
                throw new InvalidOperationException(msg);
            }

            if (cookie == null)
                throw new ArgumentNullException("cookie");

            if (!CanSet(out msg))
            {
                Console.WriteLine(msg);
                return;
            }

            lock (_forState)
            {
                if (!CanSet(out msg))
                {
                    Console.WriteLine(msg);
                    return;
                }

                lock (_cookies.SyncRoot)
                    _cookies.SetOrRemove(cookie);
            }
        }

        /// <summary>
        /// 设置 HTTP 身份验证的凭据（基本/摘要）。
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="preAuth"></param>
        public void SetCredentials(string username, string password, bool preAuth)
        {
            string msg = null;

            if (!_client)
            {
                msg = "This instance is not a client.";
                throw new InvalidOperationException(msg);
            }

            if (!username.IsNullOrEmpty())
            {
                if (username.Contains(':') || !username.IsText())
                {
                    msg = "It contains an invalid character.";
                    throw new ArgumentException(msg, "username");
                }
            }

            if (!password.IsNullOrEmpty())
            {
                if (!password.IsText())
                {
                    msg = "It contains an invalid character.";
                    throw new ArgumentException(msg, "password");
                }
            }

            if (!CanSet(out msg))
            {
                Console.WriteLine(msg);
                return;
            }

            lock (_forState)
            {
                if (!CanSet(out msg))
                {
                    Console.WriteLine(msg);
                    return;
                }

                if (username.IsNullOrEmpty())
                {
                    _credentials = null;
                    _preAuth = false;

                    return;
                }

                _credentials = new NetworkCredential(
                                 username, password, _uri.PathAndQuery
                               );

                _preAuth = preAuth;
            }
        }

        /// <summary>
        /// 设置 HTTP 代理服务器的 URL，通过它连接和HTTP 代理身份验证的凭据（基本/摘要）。
        /// </summary>
        /// <param name="url"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        public void SetProxy(string url, string username, string password)
        {
            string msg = null;

            if (!_client)
            {
                msg = "This instance is not a client.";
                throw new InvalidOperationException(msg);
            }

            Uri uri = null;

            if (!url.IsNullOrEmpty())
            {
                if (!Uri.TryCreate(url, UriKind.Absolute, out uri))
                {
                    msg = "Not an absolute URI string.";
                    throw new ArgumentException(msg, "url");
                }

                if (uri.Scheme != "http")
                {
                    msg = "The scheme part is not http.";
                    throw new ArgumentException(msg, "url");
                }

                if (uri.Segments.Length > 1)
                {
                    msg = "It includes the path segments.";
                    throw new ArgumentException(msg, "url");
                }
            }

            if (!username.IsNullOrEmpty())
            {
                if (username.Contains(':') || !username.IsText())
                {
                    msg = "It contains an invalid character.";
                    throw new ArgumentException(msg, "username");
                }
            }

            if (!password.IsNullOrEmpty())
            {
                if (!password.IsText())
                {
                    msg = "It contains an invalid character.";
                    throw new ArgumentException(msg, "password");
                }
            }

            if (!CanSet(out msg))
            {
                Console.WriteLine(msg);
                return;
            }

            lock (_forState)
            {
                if (!CanSet(out msg))
                {
                    Console.WriteLine(msg);
                    return;
                }

                if (url.IsNullOrEmpty())
                {
                    _proxyUri = null;
                    _proxyCredentials = null;

                    return;
                }

                _proxyUri = uri;
                _proxyCredentials = !username.IsNullOrEmpty()
                                    ? new NetworkCredential(
                                        username,
                                        password,
                                        String.Format(
                                          "{0}:{1}", _uri.DnsSafeHost, _uri.Port
                                        )
                                      )
                                    : null;
            }
        }

        /// <summary>
        /// 关闭连接并释放所有相关的资源
        /// </summary>
        void IDisposable.Dispose()
        {
            _Close(1001, String.Empty);
        }
    }
}
