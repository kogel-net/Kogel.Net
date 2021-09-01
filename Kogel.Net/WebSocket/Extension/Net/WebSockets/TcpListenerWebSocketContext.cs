using Kogel.Net.WebSocket.Enums;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Principal;
using System.Text;

namespace Kogel.Net.WebSocket.Extension.Net.WebSockets
{
    /// <summary>
    /// 提供对 WebSocket 握手请求中的信息的访问 一个 <see cref="TcpListener"/> 实例
    /// </summary>
    internal class TcpListenerWebSocketContext : WebSocketContext
    {
        private NameValueCollection _queryString;
        private HttpRequest _request;
        private Uri _requestUri;
        private bool _secure;
        private System.Net.EndPoint _serverEndPoint;
        private Stream _stream;
        private TcpClient _tcpClient;
        private IPrincipal _user;
        private System.Net.EndPoint _userEndPoint;
        private WebSocket _websocket;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tcpClient"></param>
        /// <param name="protocol"></param>
        /// <param name="secure"></param>
        /// <param name="sslConfig"></param>
        internal TcpListenerWebSocketContext(TcpClient tcpClient, string protocol, bool secure, ServerSslConfiguration sslConfig)
        {
            _tcpClient = tcpClient;
            _secure = secure;

            var netStream = tcpClient.GetStream();
            if (secure)
            {
                var sslStream = new SslStream(
                                  netStream,
                                  false,
                                  sslConfig.ClientCertificateValidationCallback
                                );

                sslStream.AuthenticateAsServer(
                  sslConfig.ServerCertificate,
                  sslConfig.ClientCertificateRequired,
                  sslConfig.EnabledSslProtocols,
                  sslConfig.CheckCertificateRevocation
                );

                _stream = sslStream;
            }
            else
            {
                _stream = netStream;
            }

            var sock = tcpClient.Client;
            _serverEndPoint = sock.LocalEndPoint;
            _userEndPoint = sock.RemoteEndPoint;

            _request = HttpRequest.Read(_stream, 90000);
            _websocket = new WebSocket(this, protocol);
        }

        /// <summary>
        /// 
        /// </summary>
        internal Stream Stream
        {
            get
            {
                return _stream;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        public override CookieCollection CookieCollection
        {
            get
            {
                return _request.Cookies;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override NameValueCollection Headers
        {
            get
            {
                return _request.Headers;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override string Host
        {
            get
            {
                return _request.Headers["Host"];
            }
        }

        /// <summary>
        /// 获取指示客户端是否通过身份验证的值
        /// </summary>
        public override bool IsAuthenticated
        {
            get
            {
                return _user != null;
            }
        }

        /// <summary>
        /// 获取一个值，该值指示握手请求是否从本地计算机发送
        /// </summary>
        public override bool IsLocal
        {
            get
            {
                return UserEndPoint.Address.IsLocal();
            }
        }

        /// <summary>
        /// 获取一个值，该值指示是否使用安全连接来发送握手请求
        /// </summary>
        public override bool IsSecureConnection
        {
            get
            {
                return _secure;
            }
        }

        /// <summary>
        /// 获取一个值，该值指示该请求是否为 WebSocket 握手请求
        /// </summary>
        public override bool IsWebSocketRequest
        {
            get
            {
                return _request.IsWebSocketRequest;
            }
        }

        /// <summary>
        /// 获取握手请求中包含的 Origin 标头的值
        /// </summary>
        public override string Origin
        {
            get
            {
                return _request.Headers["Origin"];
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override NameValueCollection QueryString
        {
            get
            {
                if (_queryString == null)
                {
                    var uri = RequestUri;
                    _queryString = QueryStringCollection.Parse(
                                     uri != null ? uri.Query : null,
                                     Encoding.UTF8
                                   );
                }

                return _queryString;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override Uri RequestUri
        {
            get
            {
                if (_requestUri == null)
                {
                    _requestUri = HttpUtility.CreateRequestUrl(
                                    _request.RequestUri,
                                    _request.Headers["Host"],
                                    _request.IsWebSocketRequest,
                                    _secure
                                  );
                }

                return _requestUri;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override string SecWebSocketKey
        {
            get
            {
                return _request.Headers["Sec-WebSocket-Key"];
            }
        }

       /// <summary>
       /// 
       /// </summary>
        public override IEnumerable<string> SecWebSocketProtocols
        {
            get
            {
                var val = _request.Headers["Sec-WebSocket-Protocol"];
                if (val == null || val.Length == 0)
                    yield break;

                foreach (var elm in val.Split(','))
                {
                    var protocol = elm.Trim();
                    if (protocol.Length == 0)
                        continue;

                    yield return protocol;
                }
            }
        }

       /// <summary>
       /// 
       /// </summary>
        public override string SecWebSocketVersion
        {
            get
            {
                return _request.Headers["Sec-WebSocket-Version"];
            }
        }

       /// <summary>
       /// 
       /// </summary>
        public override System.Net.IPEndPoint ServerEndPoint
        {
            get
            {
                return (System.Net.IPEndPoint)_serverEndPoint;
            }
        }

      /// <summary>
      /// 
      /// </summary>
        public override IPrincipal User
        {
            get
            {
                return _user;
            }
        }

     /// <summary>
     /// 
     /// </summary>
        public override System.Net.IPEndPoint UserEndPoint
        {
            get
            {
                return (System.Net.IPEndPoint)_userEndPoint;
            }
        }

       /// <summary>
       /// 
       /// </summary>
        public override WebSocket WebSocket
        {
            get
            {
                return _websocket;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="challenge"></param>
        /// <returns></returns>
        private HttpRequest SendAuthenticationChallenge(string challenge)
        {
            var res = HttpResponse.CreateUnauthorizedResponse(challenge);
            var bytes = res.ToByteArray();
            _stream.Write(bytes, 0, bytes.Length);

            return HttpRequest.Read(_stream, 15000);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="scheme"></param>
        /// <param name="realm"></param>
        /// <param name="credentialsFinder"></param>
        /// <returns></returns>
        internal bool Authenticate(AuthenticationSchemes scheme,string realm,Func<IIdentity, NetworkCredential> credentialsFinder)
        {
            var chal = new AuthenticationChallenge(scheme, realm).ToString();
            var retry = -1;
            Func<bool> auth = null;
            auth =
              () =>
              {
                  retry++;
                  if (retry > 99)
                      return false;

                  var user = HttpUtility.CreateUser(
                         _request.Headers["Authorization"],
                         scheme,
                         realm,
                         _request.HttpMethod,
                         credentialsFinder
                       );

                  if (user != null && user.Identity.IsAuthenticated)
                  {
                      _user = user;
                      return true;
                  }

                  _request = SendAuthenticationChallenge(chal);
                  return auth();
              };

            return auth();
        }

        /// <summary>
        /// 
        /// </summary>
        internal void Close()
        {
            _stream.Close();
            _tcpClient.Close();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="code"></param>
        internal void Close(HttpStatusCode code)
        {
            var res = HttpResponse.CreateCloseResponse(code);
            var bytes = res.ToByteArray();
            _stream.Write(bytes, 0, bytes.Length);

            _stream.Close();
            _tcpClient.Close();
        }

        /// <summary>
        /// 返回代表当前实例的字符串
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return _request.ToString();
        }
    }
}
