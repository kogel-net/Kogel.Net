using Kogel.Net.WebSocket.Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Threading;

namespace Kogel.Net.WebSocket.Extension.Net
{
    /// <summary>
    /// 提供一个 HTTP 侦听器
    /// </summary>
    public sealed class HttpListener : IDisposable
    {
        private AuthenticationSchemes _authSchemes;
        private Func<HttpListenerRequest, AuthenticationSchemes> _authSchemeSelector;
        private string _certFolderPath;
        private Queue<HttpListenerContext> _contextQueue;
        private LinkedList<HttpListenerContext> _contextRegistry;
        private object _contextRegistrySync;
        private static readonly string _defaultRealm;
        private bool _disposed;
        private bool _ignoreWriteExceptions;
        private volatile bool _listening;
        private string _objectName;
        private HttpListenerPrefixCollection _prefixes;
        private string _realm;
        private bool _reuseAddress;
        private ServerSslConfiguration _sslConfig;
        private Func<IIdentity, NetworkCredential> _userCredFinder;
        private Queue<HttpListenerAsyncResult> _waitQueue;

        static HttpListener()
        {
            _defaultRealm = "SECRET AREA";
        }

        /// <summary>
        /// 
        /// </summary>
        public HttpListener()
        {
            _authSchemes = AuthenticationSchemes.Anonymous;
            _contextQueue = new Queue<HttpListenerContext>();

            _contextRegistry = new LinkedList<HttpListenerContext>();
            _contextRegistrySync = ((ICollection)_contextRegistry).SyncRoot;

            _objectName = GetType().ToString();
            _prefixes = new HttpListenerPrefixCollection(this);
            _waitQueue = new Queue<HttpListenerAsyncResult>();
        }

        internal bool ReuseAddress
        {
            get
            {
                return _reuseAddress;
            }

            set
            {
                _reuseAddress = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public AuthenticationSchemes AuthenticationSchemes
        {
            get
            {
                if (_disposed)
                    throw new ObjectDisposedException(_objectName);

                return _authSchemes;
            }

            set
            {
                if (_disposed)
                    throw new ObjectDisposedException(_objectName);

                _authSchemes = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public Func<HttpListenerRequest, AuthenticationSchemes> AuthenticationSchemeSelector
        {
            get
            {
                if (_disposed)
                    throw new ObjectDisposedException(_objectName);

                return _authSchemeSelector;
            }

            set
            {
                if (_disposed)
                    throw new ObjectDisposedException(_objectName);

                _authSchemeSelector = value;
            }
        }

        /// <summary>
        /// 获取或设置存储用于在安全连接上对服务器进行身份验证的证书文件的文件夹的路径
        /// </summary>
        public string CertificateFolderPath
        {
            get
            {
                if (_disposed)
                    throw new ObjectDisposedException(_objectName);

                return _certFolderPath;
            }

            set
            {
                if (_disposed)
                    throw new ObjectDisposedException(_objectName);

                _certFolderPath = value;
            }
        }

        /// <summary>
        /// 获取或设置一个值，该值指示侦听器是否返回向客户端发送响应时发生的异常
        /// </summary>
        public bool IgnoreWriteExceptions
        {
            get
            {
                if (_disposed)
                    throw new ObjectDisposedException(_objectName);

                return _ignoreWriteExceptions;
            }

            set
            {
                if (_disposed)
                    throw new ObjectDisposedException(_objectName);

                _ignoreWriteExceptions = value;
            }
        }

        /// <summary>
        /// 获取一个值，该值指示侦听器是否已启动
        /// </summary>
        public bool IsListening
        {
            get
            {
                return _listening;
            }
        }

        /// <summary>
        /// 获取一个值，该值指示侦听器是否可用于当前操作系统
        /// </summary>
        public static bool IsSupported
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public HttpListenerPrefixCollection Prefixes
        {
            get
            {
                if (_disposed)
                    throw new ObjectDisposedException(_objectName);

                return _prefixes;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string Realm
        {
            get
            {
                if (_disposed)
                    throw new ObjectDisposedException(_objectName);

                return _realm;
            }

            set
            {
                if (_disposed)
                    throw new ObjectDisposedException(_objectName);

                _realm = value;
            }
        }

        /// <summary>
        /// 获取用于验证服务器和可选的客户端的 SSL 配置以进行安全连接
        /// </summary>
        public ServerSslConfiguration SslConfiguration
        {
            get
            {
                if (_disposed)
                    throw new ObjectDisposedException(_objectName);

                if (_sslConfig == null)
                    _sslConfig = new ServerSslConfiguration();

                return _sslConfig;
            }
        }

        /// <summary>
        /// 获取或设置一个值，该值指示在使用 NTLM 身份验证时，是否使用第一个请求的身份验证信息对同一连接上的其他请求进行身份验证
        /// </summary>
        public bool UnsafeConnectionNtlmAuthentication
        {
            get
            {
                throw new NotSupportedException();
            }

            set
            {
                throw new NotSupportedException();
            }
        }

        /// <summary>
        /// 获取或设置被调用以查找用于对客户端进行身份验证的身份的凭据的委托
        /// </summary>
        public Func<IIdentity, NetworkCredential> UserCredentialsFinder
        {
            get
            {
                if (_disposed)
                    throw new ObjectDisposedException(_objectName);

                return _userCredFinder;
            }

            set
            {
                if (_disposed)
                    throw new ObjectDisposedException(_objectName);

                _userCredFinder = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        private HttpListenerAsyncResult beginGetContext(AsyncCallback callback, object state)
        {
            lock (_contextRegistrySync)
            {
                if (!_listening)
                {
                    var msg = _disposed
                              ? "The listener is closed."
                              : "The listener is stopped.";

                    throw new HttpListenerException(995, msg);
                }

                var ares = new HttpListenerAsyncResult(callback, state);

                if (_contextQueue.Count == 0)
                {
                    _waitQueue.Enqueue(ares);
                }
                else
                {
                    var ctx = _contextQueue.Dequeue();
                    ares.Complete(ctx, true);
                }

                return ares;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="force"></param>
        private void cleanupContextQueue(bool force)
        {
            if (_contextQueue.Count == 0)
                return;

            if (force)
            {
                _contextQueue.Clear();

                return;
            }

            var ctxs = _contextQueue.ToArray();

            _contextQueue.Clear();

            foreach (var ctx in ctxs)
            {
                ctx.ErrorStatusCode = 503;
                ctx.SendError();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void cleanupContextRegistry()
        {
            var cnt = _contextRegistry.Count;

            if (cnt == 0)
                return;

            var ctxs = new HttpListenerContext[cnt];
            _contextRegistry.CopyTo(ctxs, 0);

            _contextRegistry.Clear();

            foreach (var ctx in ctxs)
                ctx.Connection.Close(true);
        }

        private void cleanupWaitQueue(string message)
        {
            if (_waitQueue.Count == 0)
                return;

            var aress = _waitQueue.ToArray();

            _waitQueue.Clear();

            foreach (var ares in aress)
            {
                var ex = new HttpListenerException(995, message);
                ares.Complete(ex);
            }
        }

        private void close(bool force)
        {
            if (!_listening)
            {
                _disposed = true;

                return;
            }

            _listening = false;

            cleanupContextQueue(force);
            cleanupContextRegistry();

            var msg = "The listener is closed.";
            cleanupWaitQueue(msg);

            EndPointManager.RemoveListener(this);

            _disposed = true;
        }

        private string getRealm()
        {
            var realm = _realm;

            return realm != null && realm.Length > 0 ? realm : _defaultRealm;
        }

        private AuthenticationSchemes selectAuthenticationScheme(
          HttpListenerRequest request
        )
        {
            var selector = _authSchemeSelector;

            if (selector == null)
                return _authSchemes;

            try
            {
                return selector(request);
            }
            catch
            {
                return AuthenticationSchemes.None;
            }
        }

        internal bool AuthenticateContext(HttpListenerContext context)
        {
            var req = context.Request;
            var schm = selectAuthenticationScheme(req);

            if (schm == AuthenticationSchemes.Anonymous)
                return true;

            if (schm == AuthenticationSchemes.None)
            {
                context.ErrorStatusCode = 403;
                context.ErrorMessage = "Authentication not allowed";

                context.SendError();

                return false;
            }

            var realm = getRealm();
            var user = HttpUtility.CreateUser(
                         req.Headers["Authorization"],
                         schm,
                         realm,
                         req.HttpMethod,
                         _userCredFinder
                       );

            var authenticated = user != null && user.Identity.IsAuthenticated;

            if (!authenticated)
            {
                context.SendAuthenticationChallenge(schm, realm);

                return false;
            }

            context.User = user;

            return true;
        }

        internal void CheckDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(_objectName);
        }

        internal bool RegisterContext(HttpListenerContext context)
        {
            if (!_listening)
                return false;

            lock (_contextRegistrySync)
            {
                if (!_listening)
                    return false;

                context.Listener = this;

                _contextRegistry.AddLast(context);

                if (_waitQueue.Count == 0)
                {
                    _contextQueue.Enqueue(context);
                }
                else
                {
                    var ares = _waitQueue.Dequeue();
                    ares.Complete(context, false);
                }

                return true;
            }
        }

        internal void UnregisterContext(HttpListenerContext context)
        {
            lock (_contextRegistrySync)
                _contextRegistry.Remove(context);
        }

        /// <summary>
        /// 立即关闭监听器
        /// </summary>
        public void Abort()
        {
            if (_disposed)
                return;

            lock (_contextRegistrySync)
            {
                if (_disposed)
                    return;

                close(true);
            }
        }

        /// <summary>
        /// 开始异步获取传入请求
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public IAsyncResult BeginGetContext(AsyncCallback callback, object state)
        {
            if (_disposed)
                throw new ObjectDisposedException(_objectName);

            if (_prefixes.Count == 0)
            {
                var msg = "The listener has no URI prefix on which listens.";

                throw new InvalidOperationException(msg);
            }

            if (!_listening)
            {
                var msg = "The listener has not been started.";

                throw new InvalidOperationException(msg);
            }

            return beginGetContext(callback, state);
        }

        /// <summary>
        /// 关闭监听器
        /// </summary>
        public void Close()
        {
            if (_disposed)
                return;

            lock (_contextRegistrySync)
            {
                if (_disposed)
                    return;

                close(false);
            }
        }

        /// <summary>
        /// 结束异步操作以获取传入请求
        /// </summary>
        /// <param name="asyncResult"></param>
        /// <returns></returns>
        public HttpListenerContext EndGetContext(IAsyncResult asyncResult)
        {
            if (_disposed)
                throw new ObjectDisposedException(_objectName);

            if (asyncResult == null)
                throw new ArgumentNullException("asyncResult");

            var ares = asyncResult as HttpListenerAsyncResult;

            if (ares == null)
            {
                var msg = "A wrong IAsyncResult instance.";

                throw new ArgumentException(msg, "asyncResult");
            }

            lock (ares.SyncRoot)
            {
                if (ares.EndCalled)
                {
                    var msg = "This IAsyncResult instance cannot be reused.";

                    throw new InvalidOperationException(msg);
                }

                ares.EndCalled = true;
            }

            if (!ares.IsCompleted)
                ares.AsyncWaitHandle.WaitOne();

            return ares.Context;
        }

        /// <summary>
        /// 获取传入请求
        /// </summary>
        /// <returns></returns>
        public HttpListenerContext GetContext()
        {
            if (_disposed)
                throw new ObjectDisposedException(_objectName);

            if (_prefixes.Count == 0)
            {
                var msg = "The listener has no URI prefix on which listens.";

                throw new InvalidOperationException(msg);
            }

            if (!_listening)
            {
                var msg = "The listener has not been started.";

                throw new InvalidOperationException(msg);
            }

            var ares = beginGetContext(null, null);
            ares.EndCalled = true;

            if (!ares.IsCompleted)
                ares.AsyncWaitHandle.WaitOne();

            return ares.Context;
        }

        /// <summary>
        /// 开始接收传入请求
        /// </summary>
        public void Start()
        {
            if (_disposed)
                throw new ObjectDisposedException(_objectName);

            lock (_contextRegistrySync)
            {
                if (_disposed)
                    throw new ObjectDisposedException(_objectName);

                if (_listening)
                    return;

                EndPointManager.AddListener(this);

                _listening = true;
            }
        }

        /// <summary>
        /// 止接收传入请求
        /// </summary>
        public void Stop()
        {
            if (_disposed)
                throw new ObjectDisposedException(_objectName);

            lock (_contextRegistrySync)
            {
                if (!_listening)
                    return;

                _listening = false;

                cleanupContextQueue(false);
                cleanupContextRegistry();

                var msg = "The listener is stopped.";
                cleanupWaitQueue(msg);

                EndPointManager.RemoveListener(this);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        void IDisposable.Dispose()
        {
            if (_disposed)
                return;

            lock (_contextRegistrySync)
            {
                if (_disposed)
                    return;

                close(true);
            }
        }
    }
}
