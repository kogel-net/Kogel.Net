using Kogel.Net.WebSocket.Enums;
using Kogel.Net.WebSocket.Extension.Net;
using Kogel.Net.WebSocket.Extension.Net.WebSockets;
using Kogel.Net.WebSocket.Server;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Kogel.Net.WebSocket.Extension
{
    /// <summary>
    /// 提供一个简单的 HTTP 服务器。
    /// </summary>
    public class HttpServer
    {
        private System.Net.IPAddress _address;
        private string _docRootPath;
        private string _hostname;
        private HttpListener _listener;
        private int _port;
        private Thread _receiveThread;
        private bool _secure;
        private WebSocketServiceManager _services;
        private volatile ServerState _state;
        private object _sync;

        /// <summary>
        /// 
        /// </summary>
        public HttpServer()
        {
            Init("*", System.Net.IPAddress.Any, 80, false);
        }

        /// <summary>
        /// 用指定的端口初始化 <see cref="HttpServer"/> 类的新实例
        /// </summary>
        /// <param name="port"></param>
        public HttpServer(int port)
          : this(port, port == 443)
        {
        }

        /// <summary>
        /// 使用指定的 URL 初始化 <see cref="HttpServer"/> 类的新实例
        /// </summary>
        /// <param name="url"></param>
        public HttpServer(string url)
        {
            if (url == null)
                throw new ArgumentNullException("url");

            if (url.Length == 0)
                throw new ArgumentException("An empty string.", "url");

            Uri uri;
            string msg;

            if (!TryCreateUri(url, out uri, out msg))
                throw new ArgumentException(msg, "url");

            var host = uri.GetDnsSafeHost(true);
            var addr = host.ToIPAddress();

            if (addr == null)
            {
                msg = "The host part could not be converted to an IP address.";

                throw new ArgumentException(msg, "url");
            }

            if (!addr.IsLocal())
            {
                msg = "The IP address of the host is not a local IP address.";

                throw new ArgumentException(msg, "url");
            }

            Init(host, addr, uri.Port, uri.Scheme == "https");
        }

        /// <summary>
        /// 使用指定的端口和布尔值（如果安全与否）初始化 <see cref="HttpServer"/> 类的新实例。
        /// </summary>
        /// <param name="port"></param>
        /// <param name="secure"></param>
        public HttpServer(int port, bool secure)
        {
            if (!port.IsPortNumber())
            {
                var msg = "It is less than 1 or greater than 65535.";

                throw new ArgumentOutOfRangeException("port", msg);
            }

            Init("*", System.Net.IPAddress.Any, port, secure);
        }

        /// <summary>
        /// 使用指定的 IP 地址和端口初始化 <see cref="HttpServer"/> 类的新实例
        /// </summary>
        /// <param name="address"></param>
        /// <param name="port"></param>
        public HttpServer(System.Net.IPAddress address, int port)
          : this(address, port, port == 443)
        {
        }

        /// <summary>
        /// 使用指定的 IP 地址、端口和布尔值（如果安全与否）初始化 <see cref="HttpServer"/> 类的新实例
        /// </summary>
        /// <param name="address"></param>
        /// <param name="port"></param>
        /// <param name="secure"></param>
        public HttpServer(System.Net.IPAddress address, int port, bool secure)
        {
            if (address == null)
                throw new ArgumentNullException("address");

            if (!address.IsLocal())
            {
                var msg = "It is not a local IP address.";

                throw new ArgumentException(msg, "address");
            }

            if (!port.IsPortNumber())
            {
                var msg = "It is less than 1 or greater than 65535.";

                throw new ArgumentOutOfRangeException("port", msg);
            }

            Init(address.ToString(true), address, port, secure);
        }

        /// <summary>
        /// 获取服务器的IP地址
        /// </summary>
        public System.Net.IPAddress Address
        {
            get
            {
                return _address;
            }
        }

        /// <summary>
        /// 获取或设置用于验证客户端的方案
        /// </summary>
        public AuthenticationSchemes AuthenticationSchemes
        {
            get
            {
                return _listener.AuthenticationSchemes;
            }

            set
            {
                lock (_sync)
                {
                    string msg;

                    if (!CanSet(out msg))
                    {
                        Console.WriteLine(msg);

                        return;
                    }

                    _listener.AuthenticationSchemes = value;
                }
            }
        }

        /// <summary>
        /// 获取或设置服务器文档文件夹的路径
        /// </summary>
        public string DocumentRootPath
        {
            get
            {
                return _docRootPath;
            }

            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");

                if (value.Length == 0)
                    throw new ArgumentException("An empty string.", "value");

                value = value.TrimSlashOrBackslashFromEnd();

                if (value == "/")
                    throw new ArgumentException("An absolute root.", "value");

                if (value == "\\")
                    throw new ArgumentException("An absolute root.", "value");

                if (value.Length == 2 && value[1] == ':')
                    throw new ArgumentException("An absolute root.", "value");

                string full = null;

                try
                {
                    full = Path.GetFullPath(value);
                }
                catch (Exception ex)
                {
                    throw new ArgumentException("An invalid path string.", "value", ex);
                }

                if (full == "/")
                    throw new ArgumentException("An absolute root.", "value");

                full = full.TrimSlashOrBackslashFromEnd();

                if (full.Length == 2 && full[1] == ':')
                    throw new ArgumentException("An absolute root.", "value");

                lock (_sync)
                {
                    string msg;

                    if (!CanSet(out msg))
                    {
                        Console.WriteLine(msg);

                        return;
                    }

                    _docRootPath = value;
                }
            }
        }

        /// <summary>
        /// 获取指示服务器是否已启动的值
        /// </summary>
        public bool IsListening
        {
            get
            {
                return _state == ServerState.Start;
            }
        }

        /// <summary>
        /// 获取指示是否提供安全连接的值
        /// </summary>
        public bool IsSecure
        {
            get
            {
                return _secure;
            }
        }

        /// <summary>
        /// 获取或设置一个值，该值指示服务器是否定期清理非活动会话
        /// </summary>
        public bool KeepClean
        {
            get
            {
                return _services.KeepClean;
            }

            set
            {
                _services.KeepClean = value;
            }
        }

        /// <summary>
        /// 获取服务器的端口
        /// </summary>
        public int Port
        {
            get
            {
                return _port;
            }
        }

        /// <summary>
        /// 获取或设置用于身份验证的领域
        /// </summary>
        public string Realm
        {
            get
            {
                return _listener.Realm;
            }

            set
            {
                lock (_sync)
                {
                    string msg;

                    if (!CanSet(out msg))
                    {
                        Console.WriteLine(msg);

                        return;
                    }

                    _listener.Realm = value;
                }
            }
        }

        /// <summary>
        /// 获取或设置一个值，该值指示是否允许将服务器绑定到已在使用的地址
        /// </summary>
        public bool ReuseAddress
        {
            get
            {
                return _listener.ReuseAddress;
            }

            set
            {
                lock (_sync)
                {
                    string msg;

                    if (!CanSet(out msg))
                    {
                        Console.WriteLine(msg);

                        return;
                    }

                    _listener.ReuseAddress = value;
                }
            }
        }

        /// <summary>
        /// 获取安全连接的配置
        /// </summary>
        public ServerSslConfiguration SslConfiguration
        {
            get
            {
                if (!_secure)
                {
                    var msg = "The server does not provide secure connections.";

                    throw new InvalidOperationException(msg);
                }

                return _listener.SslConfiguration;
            }
        }

        /// <summary>
        /// 获取或设置用于查找身份凭据的委托
        /// </summary>
        public Func<IIdentity, NetworkCredential> UserCredentialsFinder
        {
            get
            {
                return _listener.UserCredentialsFinder;
            }

            set
            {
                lock (_sync)
                {
                    string msg;

                    if (!CanSet(out msg))
                    {
                        Console.WriteLine(msg);

                        return;
                    }

                    _listener.UserCredentialsFinder = value;
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
                return _services.WaitTime;
            }

            set
            {
                _services.WaitTime = value;
            }
        }

        /// <summary>
        /// 获取服务器提供的WebSocket服务的管理函数
        /// </summary>
        public WebSocketServiceManager WebSocketServices
        {
            get
            {
                return _services;
            }
        }

        /// <summary>
        /// 当服务器收到 HTTP CONNECT 请求时发生.
        /// </summary>
        public event EventHandler<HttpRequestEventArgs> OnConnect;

        /// <summary>
        /// 当服务器收到 HTTP Delete 请求时发生
        /// </summary>
        public event EventHandler<HttpRequestEventArgs> OnDelete;

        /// <summary>
        /// 
        /// </summary>
        public event EventHandler<HttpRequestEventArgs> OnGet;

        /// <summary>
        /// 
        /// </summary>
        public event EventHandler<HttpRequestEventArgs> OnHead;

        /// <summary>
        /// 
        /// </summary>
        public event EventHandler<HttpRequestEventArgs> OnOptions;

        /// <summary>
        /// 
        /// </summary>
        public event EventHandler<HttpRequestEventArgs> OnPost;

        /// <summary>
        /// 
        /// </summary>
        public event EventHandler<HttpRequestEventArgs> OnPut;

        /// <summary>
        /// 
        /// </summary>
        public event EventHandler<HttpRequestEventArgs> OnTrace;

        /// <summary>
        /// 
        /// </summary>
        private void Abort()
        {
            lock (_sync)
            {
                if (_state != ServerState.Start)
                    return;

                _state = ServerState.ShuttingDown;
            }

            try
            {
                try
                {
                    _services.Stop(1006, String.Empty);
                }
                finally
                {
                    _listener.Abort();
                }
            }
            catch
            {
            }

            _state = ServerState.Stop;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private bool CanSet(out string message)
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

        private bool CheckCertificate(out string message)
        {
            message = null;

            var byUser = _listener.SslConfiguration.ServerCertificate != null;

            var path = _listener.CertificateFolderPath;
            var withPort = EndPointListener.CertificateExists(_port, path);

            var either = byUser || withPort;

            if (!either)
            {
                message = "There is no server certificate for secure connection.";

                return false;
            }

            var both = byUser && withPort;

            if (both)
            {
                var msg = "The server certificate associated with the port is used.";

                Console.WriteLine(msg);
            }

            return true;
        }

        private static HttpListener CreateListener(
          string hostname, int port, bool secure
        )
        {
            var lsnr = new HttpListener();

            var schm = secure ? "https" : "http";
            var pref = String.Format("{0}://{1}:{2}/", schm, hostname, port);
            lsnr.Prefixes.Add(pref);

            return lsnr;
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="hostname"></param>
        /// <param name="address"></param>
        /// <param name="port"></param>
        /// <param name="secure"></param>
        private void Init(
          string hostname, System.Net.IPAddress address, int port, bool secure
        )
        {
            _hostname = hostname;
            _address = address;
            _port = port;
            _secure = secure;

            _docRootPath = "./Public";
            _listener = CreateListener(_hostname, _port, _secure);
            _services = new WebSocketServiceManager();
            _sync = new object();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        private void ProcessRequest(HttpListenerContext context)
        {
            var method = context.Request.HttpMethod;
            var evt = method == "GET"
                      ? OnGet
                      : method == "HEAD"
                        ? OnHead
                        : method == "POST"
                          ? OnPost
                          : method == "PUT"
                            ? OnPut
                            : method == "DELETE"
                              ? OnDelete
                              : method == "CONNECT"
                                ? OnConnect
                                : method == "OPTIONS"
                                  ? OnOptions
                                  : method == "TRACE"
                                    ? OnTrace
                                    : null;

            if (evt != null)
                evt(this, new HttpRequestEventArgs(context, _docRootPath));
            else
                context.Response.StatusCode = 501; // Not Implemented

            context.Response.Close();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        private void ProcessRequest(HttpListenerWebSocketContext context)
        {
            var uri = context.RequestUri;

            if (uri == null)
            {
                context.Close(HttpStatusCode.BadRequest);

                return;
            }

            var path = uri.AbsolutePath;

            if (path.IndexOfAny(new[] { '%', '+' }) > -1)
                path = HttpUtility.UrlDecode(path, Encoding.UTF8);

            WebSocketServiceHost host;

            if (!_services.InternalTryGetServiceHost(path, out host))
            {
                context.Close(HttpStatusCode.NotImplemented);

                return;
            }

            host.StartSession(context);
        }

        /// <summary>
        /// 接收请求
        /// </summary>
        private void ReceiveRequest()
        {
            while (true)
            {
                HttpListenerContext ctx = null;

                try
                {
                    ctx = _listener.GetContext();

                    ThreadPool.QueueUserWorkItem(
                      state =>
                      {
                          try
                          {
                              if (ctx.Request.IsUpgradeRequest("websocket"))
                              {
                                  ProcessRequest(ctx.GetWebSocketContext(null));
                                  return;
                              }
                              ProcessRequest(ctx);
                          }
                          catch (Exception ex)
                          {
                              Console.WriteLine(ex.Message);
                              ctx.Connection.Close(true);
                          }
                      }
                    );
                }
                catch (HttpListenerException)
                {
                    Console.WriteLine("The underlying listener is stopped.");

                    break;
                }
                catch (InvalidOperationException)
                {
                    Console.WriteLine("The underlying listener is stopped.");

                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    if (ctx != null)
                        ctx.Connection.Close(true);

                    break;
                }
            }

            if (_state != ServerState.ShuttingDown)
                Abort();
        }

        private void _Start()
        {
            lock (_sync)
            {
                if (_state == ServerState.Start)
                {
                    Console.WriteLine("The server has already started.");

                    return;
                }

                if (_state == ServerState.ShuttingDown)
                {
                    Console.WriteLine("The server is shutting down.");

                    return;
                }

                _services.Start();

                try
                {
                    StartReceiving();
                }
                catch
                {
                    _services.Stop(1011, String.Empty);

                    throw;
                }

                _state = ServerState.Start;
            }
        }

        /// <summary>
        /// 启动接收
        /// </summary>
        private void StartReceiving()
        {
            try
            {
                _listener.Start();
            }
            catch (Exception ex)
            {
                var msg = "The underlying listener has failed to start.";

                throw new InvalidOperationException(msg, ex);
            }

            _receiveThread = new Thread(new ThreadStart(ReceiveRequest));
            _receiveThread.IsBackground = true;

            _receiveThread.Start();
        }

        /// <summary>
        /// 停止
        /// </summary>
        /// <param name="code"></param>
        /// <param name="reason"></param>
        private void Stop(ushort code, string reason)
        {
            lock (_sync)
            {
                if (_state == ServerState.ShuttingDown)
                {
                    Console.WriteLine("The server is shutting down.");

                    return;
                }

                if (_state == ServerState.Stop)
                {
                    Console.WriteLine("The server has already stopped.");

                    return;
                }

                _state = ServerState.ShuttingDown;
            }

            try
            {
                var threw = false;

                try
                {
                    _services.Stop(code, reason);
                }
                catch
                {
                    threw = true;

                    throw;
                }
                finally
                {
                    try
                    {
                        StopReceiving(5000);
                    }
                    catch
                    {
                        if (!threw)
                            throw;
                    }
                }
            }
            finally
            {
                _state = ServerState.Stop;
            }
        }

        /// <summary>
        /// 停止接收
        /// </summary>
        /// <param name="millisecondsTimeout"></param>
        private void StopReceiving(int millisecondsTimeout)
        {
            _listener.Stop();
            _receiveThread.Join(millisecondsTimeout);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="uriString"></param>
        /// <param name="result"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        private static bool TryCreateUri(string uriString, out Uri result, out string message)
        {
            result = null;
            message = null;

            var uri = uriString.ToUri();

            if (uri == null)
            {
                message = "An invalid URI string.";

                return false;
            }

            if (!uri.IsAbsoluteUri)
            {
                message = "A relative URI.";

                return false;
            }

            var schm = uri.Scheme;
            var http = schm == "http" || schm == "https";

            if (!http)
            {
                message = "The scheme part is not 'http' or 'https'.";

                return false;
            }

            if (uri.PathAndQuery != "/")
            {
                message = "It includes either or both path and query components.";

                return false;
            }

            if (uri.Fragment.Length > 0)
            {
                message = "It includes the fragment component.";

                return false;
            }

            if (uri.Port == 0)
            {
                message = "The port part is zero.";

                return false;
            }

            result = uri;

            return true;
        }

        /// <summary>
        /// 添加具有指定行为和路径的 WebSocket 服务
        /// </summary>
        /// <typeparam name="TBehavior"></typeparam>
        /// <param name="path"></param>
        public void AddWebSocketService<TBehavior>(string path)
          where TBehavior : WebSocketControllerBase, new()
        {
            _services.AddService<TBehavior>(path, null);
        }

        /// <summary>
        /// 添加具有指定行为和路径的 WebSocket 服务
        /// </summary>
        /// <typeparam name="TBehavior"></typeparam>
        /// <param name="path"></param>
        /// <param name="initializer"></param>
        public void AddWebSocketService<TBehavior>(string path, Action<TBehavior> initializer)
          where TBehavior : WebSocketControllerBase, new()
        {
            _services.AddService<TBehavior>(path, initializer);
        }

        /// <summary>
        /// 移除具有指定行为和路径的 WebSocket 服务
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public bool RemoveWebSocketService(string path)
        {
            return _services.RemoveService(path);
        }

       /// <summary>
       /// 
       /// </summary>
        public void Start()
        {
            if (_secure)
            {
                string msg;

                if (!CheckCertificate(out msg))
                    throw new InvalidOperationException(msg);
            }

            if (_state == ServerState.Start)
            {
                Console.WriteLine("The server has already started.");

                return;
            }

            if (_state == ServerState.ShuttingDown)
            {
                Console.WriteLine("The server is shutting down.");

                return;
            }

            _Start();
        }

        /// <summary>
        /// 止接收传入请求
        /// </summary>
        public void Stop()
        {
            if (_state == ServerState.Ready)
            {
                Console.WriteLine("The server is not started.");

                return;
            }

            if (_state == ServerState.ShuttingDown)
            {
                Console.WriteLine("The server is shutting down.");

                return;
            }

            if (_state == ServerState.Stop)
            {
                Console.WriteLine("The server has already stopped.");

                return;
            }

            Stop(1001, String.Empty);
        }
    }
}
