using Kogel.Net.WebSocket.Enums;
using Kogel.Net.WebSocket.Extension;
using Kogel.Net.WebSocket.Extension.Net;
using Kogel.Net.WebSocket.Extension.Net.WebSockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Kogel.Net.WebSocket.Server
{
    /// <summary>
    /// 提供 WebSocket 协议服务器。
    /// </summary>
    public class WebSocketServer
    {
        private System.Net.IPAddress _address;
        private bool _allowForwardedRequest;
        private AuthenticationSchemes _authSchemes;
        private static readonly string _defaultRealm;
        private bool _dnsStyle;
        private string _hostname;
        private TcpListener _listener;
        private int _port;
        private string _realm;
        private string _realmInUse;
        private Thread _receiveThread;
        private bool _reuseAddress;
        private bool _secure;
        private WebSocketServiceManager _services;
        private ServerSslConfiguration _sslConfig;
        private ServerSslConfiguration _sslConfigInUse;
        private volatile ServerState _state;
        private object _sync;
        private Func<IIdentity, NetworkCredential> _userCredFinder;

        /// <summary>
        /// 
        /// </summary>
        static WebSocketServer()
        {
            _defaultRealm = "SECRET AREA";
        }

        /// <summary>
        /// 
        /// </summary>
        public WebSocketServer()
        {
            var addr = System.Net.IPAddress.Any;
            Init(addr.ToString(), addr, 80, false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="port"></param>
        public WebSocketServer(int port) : this(port, port == 443)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        public WebSocketServer(string url)
        {
            if (url == null)
                throw new ArgumentNullException("url");

            if (url.Length == 0)
                throw new ArgumentException("An empty string.", "url");

            Uri uri;
            string msg;
            if (!TryCreateUri(url, out uri, out msg))
                throw new ArgumentException(msg, "url");

            var host = uri.DnsSafeHost;

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

            Init(host, addr, uri.Port, uri.Scheme == "wss");
        }

        /// <summary>
        /// 初始化类端口的新实例并保护
        /// </summary>
        /// <param name="port"></param>
        /// <param name="secure"></param>
        public WebSocketServer(int port, bool secure)
        {
            if (!port.IsPortNumber())
            {
                var msg = "Less than 1 or greater than 65535.";
                throw new ArgumentOutOfRangeException("port", msg);
            }

            var addr = System.Net.IPAddress.Any;
            Init(addr.ToString(), addr, port, secure);
        }

        /// <summary>
        /// 用指定的值初始化类的新实例
        /// </summary>
        /// <param name="address"></param>
        /// <param name="port"></param>
        public WebSocketServer(System.Net.IPAddress address, int port) : this(address, port, port == 443)
        {
        }

        /// <summary>
        /// 用指定的值初始化类的新实例
        /// </summary>
        /// <param name="address"></param>
        /// <param name="port"></param>
        /// <param name="secure"></param>
        public WebSocketServer(System.Net.IPAddress address, int port, bool secure)
        {
            if (address == null)
                throw new ArgumentNullException("address");

            if (!address.IsLocal())
                throw new ArgumentException("Not a local IP address.", "address");

            if (!port.IsPortNumber())
            {
                var msg = "Less than 1 or greater than 65535.";
                throw new ArgumentOutOfRangeException("port", msg);
            }

            Init(address.ToString(), address, port, secure);
        }

        /// <summary>
        /// 获取服务器的 IP 地址。
        /// </summary>
        public System.Net.IPAddress Address
        {
            get
            {
                return _address;
            }
        }

        /// <summary>
        /// 获取或设置一个值，该值指示服务器是否在不检查请求 URI 的情况下接受每个握手请求。
        /// </summary>
        public bool AllowForwardedRequest
        {
            get
            {
                return _allowForwardedRequest;
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

                    _allowForwardedRequest = value;
                }
            }
        }

        /// <summary>
        /// 获取或设置用于对客户端进行身份验证的方案。
        /// </summary>
        public AuthenticationSchemes AuthenticationSchemes
        {
            get
            {
                return _authSchemes;
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

                    _authSchemes = value;
                }
            }
        }

        /// <summary>
        /// 获取一个值，该值指示服务器是否已启动。
        /// </summary>
        public bool IsListening
        {
            get
            {
                return _state == ServerState.Start;
            }
        }

        /// <summary>
        /// 获取一个值，该值指示是否提供安全连接。
        /// </summary>
        public bool IsSecure
        {
            get
            {
                return _secure;
            }
        }

        /// <summary>
        /// 获取或设置一个值，该值指示服务器是否定期清理非活动会话。
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
        /// 获取服务器的端口。
        /// </summary>
        public int Port
        {
            get
            {
                return _port;
            }
        }

        /// <summary>
        /// 获取或设置用于身份验证的领域。
        /// </summary>
        public string Realm
        {
            get
            {
                return _realm;
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

                    _realm = value;
                }
            }
        }

        /// <summary>
        /// 获取或设置一个值，该值指示是否允许将服务器绑定到已在使用的地址。
        /// </summary>
        public bool ReuseAddress
        {
            get
            {
                return _reuseAddress;
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

                    _reuseAddress = value;
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
                    var msg = "This instance does not provide secure connections.";
                    throw new InvalidOperationException(msg);
                }

                return GetSslConfiguration();
            }
        }

        /// <summary>
        /// 获取或设置用于查找身份凭据的委托。
        /// </summary>
        public Func<IIdentity, NetworkCredential> UserCredentialsFinder
        {
            get
            {
                return _userCredFinder;
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

                    _userCredFinder = value;
                }
            }
        }

        /// <summary>
        /// 获取或设置等待响应 WebSocket Ping 或 Close 的时间。
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
        /// 获取服务器提供的WebSocket服务的管理函数。
        /// </summary>
        public WebSocketServiceManager WebSocketServices
        {
            get
            {
                return _services;
            }
        }

        /// <summary>
        /// 终止服务
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
                    _listener.Stop();
                }
                finally
                {
                    _services.Stop(1006, String.Empty);
                }
            }
            catch
            {
            }

            _state = ServerState.Stop;
        }

        /// <summary>
        /// 授权连接
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private bool AuthenticateClient(TcpListenerWebSocketContext context)
        {
            if (_authSchemes == AuthenticationSchemes.Anonymous)
                return true;

            if (_authSchemes == AuthenticationSchemes.None)
                return false;

            return context.Authenticate(_authSchemes, _realmInUse, _userCredFinder);
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

        /// <summary>
        /// 从url中确定host name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private bool CheckHostNameForRequest(string name)
        {
            return !_dnsStyle
                   || Uri.CheckHostName(name) != UriHostNameType.Dns
                   || name == _hostname;
        }

        /// <summary>
        ///确定加密配置
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        private static bool CheckSslConfiguration(ServerSslConfiguration configuration, out string message)
        {
            message = null;

            if (configuration.ServerCertificate == null)
            {
                message = "There is no server certificate for secure connection.";
                return false;
            }

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private string GetRealm()
        {
            var realm = _realm;
            return realm != null && realm.Length > 0 ? realm : _defaultRealm;
        }

        /// <summary>
        /// 获取加密配置
        /// </summary>
        /// <returns></returns>
        private ServerSslConfiguration GetSslConfiguration()
        {
            if (_sslConfig == null)
                _sslConfig = new ServerSslConfiguration();

            return _sslConfig;
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="hostname"></param>
        /// <param name="address"></param>
        /// <param name="port"></param>
        /// <param name="secure"></param>
        private void Init(string hostname, System.Net.IPAddress address, int port, bool secure)
        {
            _hostname = hostname;
            _address = address;
            _port = port;
            _secure = secure;

            _authSchemes = AuthenticationSchemes.Anonymous;
            _dnsStyle = Uri.CheckHostName(hostname) == UriHostNameType.Dns;
            _listener = new TcpListener(address, port);
            _services = new WebSocketServiceManager();
            _sync = new object();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        private void ProcessRequest(TcpListenerWebSocketContext context)
        {
            if (!AuthenticateClient(context))
            {
                context.Close(HttpStatusCode.Forbidden);
                return;
            }

            var uri = context.RequestUri;
            if (uri == null)
            {
                context.Close(HttpStatusCode.BadRequest);
                return;
            }

            if (!_allowForwardedRequest)
            {
                if (uri.Port != _port)
                {
                    context.Close(HttpStatusCode.BadRequest);
                    return;
                }

                if (!CheckHostNameForRequest(uri.DnsSafeHost))
                {
                    context.Close(HttpStatusCode.NotFound);
                    return;
                }
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
                TcpClient cl = null;
                try
                {
                    cl = _listener.AcceptTcpClient();
                    ThreadPool.QueueUserWorkItem(
                      state =>
                      {
                          try
                          {
                              var ctx = new TcpListenerWebSocketContext(
                                cl, null, _secure, _sslConfigInUse
                              );

                              ProcessRequest(ctx);
                          }
                          catch (Exception ex)
                          {
                              Console.WriteLine(ex.Message);
                              cl.Close();
                          }
                      }
                    );
                }
                catch (SocketException ex)
                {
                    if (_state == ServerState.ShuttingDown)
                    {
                        Console.WriteLine("The underlying listener is stopped.");
                        break;
                    }

                    Console.WriteLine(ex.Message);

                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);

                    if (cl != null)
                        cl.Close();

                    break;
                }
            }

            if (_state != ServerState.ShuttingDown)
                Abort();
        }

        /// <summary>
        /// 启动（带ssl配置）
        /// </summary>
        /// <param name="sslConfig"></param>
        private void Start(ServerSslConfiguration sslConfig)
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

                _sslConfigInUse = sslConfig;
                _realmInUse = GetRealm();

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
        /// 开始接收
        /// </summary>
        private void StartReceiving()
        {
            if (_reuseAddress)
            {
                _listener.Server.SetSocketOption(
                  SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true
                );
            }

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
        /// 停止指定会话
        /// </summary>
        /// <param name="code"></param>
        /// <param name="reason"></param>
        private void _Stop(ushort code, string reason)
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
                    StopReceiving(5000);
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
                        _services.Stop(code, reason);
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
        /// 
        /// </summary>
        /// <param name="millisecondsTimeout"></param>
        private void StopReceiving(int millisecondsTimeout)
        {
            try
            {
                _listener.Stop();
            }
            catch (Exception ex)
            {
                var msg = "The underlying listener has failed to stop.";
                throw new InvalidOperationException(msg, ex);
            }

            _receiveThread.Join(millisecondsTimeout);
        }

        private static bool TryCreateUri(string uriString, out Uri result, out string message )
        {
            if (!uriString.TryCreateWebSocketUri(out result, out message))
                return false;

            if (result.PathAndQuery != "/")
            {
                result = null;
                message = "It includes either or both path and query components.";

                return false;
            }

            return true;
        }

        /// <summary>
        /// 添加具有指定行为和路径的 WebSocket 服务。
        /// </summary>
        /// <typeparam name="TBehaviorWithNew"></typeparam>
        /// <param name="path"></param>
        public void AddController<TBehaviorWithNew>(string path)
          where TBehaviorWithNew : WebSocketControllerBase, new()
        {
            _services.AddService<TBehaviorWithNew>(path, null);
        }

        /// <summary>
        /// 添加具有指定行为、路径和委托的 WebSocket 服务
        /// </summary>
        /// <typeparam name="TBehaviorWithNew"></typeparam>
        /// <param name="path"></param>
        /// <param name="initializer"></param>
        public void AddController<TBehaviorWithNew>(
          string path, Action<TBehaviorWithNew> initializer
        )
          where TBehaviorWithNew : WebSocketControllerBase, new()
        {
            _services.AddService<TBehaviorWithNew>(path, initializer);
        }

        /// <summary>
        /// 删除指定路径的 WebSocket 服务
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public bool RemoveWebSocketService(string path)
        {
            return _services.RemoveService(path);
        }

        /// <summary>
        /// 开始接收传入的握手请求
        /// </summary>
        public void Start()
        {
            ServerSslConfiguration sslConfig = null;

            if (_secure)
            {
                sslConfig = new ServerSslConfiguration(GetSslConfiguration());

                string msg;
                if (!CheckSslConfiguration(sslConfig, out msg))
                    throw new InvalidOperationException(msg);
            }

            Start(sslConfig);
        }

        /// <summary>
        /// 停止接收传入的握手请求
        /// </summary>
        public void Stop()
        {
            _Stop(1001, String.Empty);
        }
    }
}
