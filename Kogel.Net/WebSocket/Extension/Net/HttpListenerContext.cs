using System;
using System.Security.Principal;
using System.Text;
using Kogel.Net.WebSocket.Enums;
using Kogel.Net.WebSocket.Extension.Net.WebSockets;

namespace Kogel.Net.WebSocket.Extension.Net
{
    /// <summary>
    /// 提供对 <see cref="HttpListener"/> 类使用的 HTTP 请求和响应对象的访问
    /// </summary>
    public sealed class HttpListenerContext
    {
        private HttpConnection _connection;
        private string _errorMessage;
        private int _errorStatusCode;
        private HttpListener _listener;
        private HttpListenerRequest _request;
        private HttpListenerResponse _response;
        private IPrincipal _user;
        private HttpListenerWebSocketContext _websocketContext;
        internal HttpListenerContext(HttpConnection connection)
        {
            _connection = connection;

            _errorStatusCode = 400;
            _request = new HttpListenerRequest(this);
            _response = new HttpListenerResponse(this);
        }

        internal HttpConnection Connection
        {
            get
            {
                return _connection;
            }
        }

        internal string ErrorMessage
        {
            get
            {
                return _errorMessage;
            }

            set
            {
                _errorMessage = value;
            }
        }

        internal int ErrorStatusCode
        {
            get
            {
                return _errorStatusCode;
            }

            set
            {
                _errorStatusCode = value;
            }
        }

        internal bool HasErrorMessage
        {
            get
            {
                return _errorMessage != null;
            }
        }

        internal HttpListener Listener
        {
            get
            {
                return _listener;
            }

            set
            {
                _listener = value;
            }
        }

        /// <summary>
        /// 获取代表客户端请求的 HTTP 请求对象
        /// </summary>
        public HttpListenerRequest Request
        {
            get
            {
                return _request;
            }
        }

        /// <summary>
        /// 获取用于向客户端发送响应的 HTTP 响应对象
        /// </summary>
        public HttpListenerResponse Response
        {
            get
            {
                return _response;
            }
        }

        /// <summary>
        /// 获取客户端信息（身份、身份验证和安全角色）
        /// </summary>
        public IPrincipal User
        {
            get
            {
                return _user;
            }

            internal set
            {
                _user = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="statusCode"></param>
        /// <param name="statusDescription"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        private static string createErrorContent(int statusCode, string statusDescription, string message)
        {
            return message != null && message.Length > 0
                   ? String.Format(
                       "<html><body><h1>{0} {1} ({2})</h1></body></html>",
                       statusCode,
                       statusDescription,
                       message
                     )
                   : String.Format(
                       "<html><body><h1>{0} {1}</h1></body></html>",
                       statusCode,
                       statusDescription
                     );
        }

        internal HttpListenerWebSocketContext GetWebSocketContext(string protocol)
        {
            _websocketContext = new HttpListenerWebSocketContext(this, protocol);
            return _websocketContext;
        }

        internal void SendAuthenticationChallenge(AuthenticationSchemes scheme, string realm)
        {
            var chal = new AuthenticationChallenge(scheme, realm).ToString();

            _response.StatusCode = 401;
            _response.Headers.InternalSet("WWW-Authenticate", chal, true);

            _response.Close();
        }

        internal void SendError()
        {
            try
            {
                _response.StatusCode = _errorStatusCode;
                _response.ContentType = "text/html";

                var content = createErrorContent(
                                _errorStatusCode,
                                _response.StatusDescription,
                                _errorMessage
                              );

                var enc = Encoding.UTF8;
                var entity = enc.GetBytes(content);
                _response.ContentEncoding = enc;
                _response.ContentLength64 = entity.LongLength;

                _response.Close(entity, true);
            }
            catch
            {
                _connection.Close(true);
            }
        }

        internal void Unregister()
        {
            if (_listener == null)
                return;

            _listener.UnregisterContext(this);
        }





        /// <summary>
        /// 接受 WebSocket 握手请求
        /// </summary>
        /// <param name="protocol"></param>
        /// <returns></returns>
        public HttpListenerWebSocketContext AcceptWebSocket(string protocol)
        {
            if (_websocketContext != null)
            {
                var msg = "The accepting is already in progress.";

                throw new InvalidOperationException(msg);
            }

            if (protocol != null)
            {
                if (protocol.Length == 0)
                {
                    var msg = "An empty string.";

                    throw new ArgumentException(msg, "protocol");
                }

                if (!protocol.IsToken())
                {
                    var msg = "It contains an invalid character.";

                    throw new ArgumentException(msg, "protocol");
                }
            }

            return GetWebSocketContext(protocol);
        }
    }
}
