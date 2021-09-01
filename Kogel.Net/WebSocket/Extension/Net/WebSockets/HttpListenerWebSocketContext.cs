using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Security.Principal;

namespace Kogel.Net.WebSocket.Extension.Net.WebSockets
{
    /// <summary>
    /// 提供对 <see cref="HttpListener"/> 实例的 WebSocket 握手请求中的信息的访问
    /// </summary>
    public class HttpListenerWebSocketContext : WebSocketContext
    {
        private HttpListenerContext _context;
        private WebSocket _websocket;

        internal HttpListenerWebSocketContext(HttpListenerContext context, string protocol)
        {
            _context = context;
            _websocket = new WebSocket(this, protocol);
        }

        internal Stream Stream
        {
            get
            {
                return _context.Connection.Stream;
            }
        }

        /// <summary>
        /// 获取握手请求中包含的 HTTP cookie
        /// </summary>
        public override CookieCollection CookieCollection
        {
            get
            {
                return _context.Request.Cookies;
            }
        }

        /// <summary>
        /// 获取握手请求中包含的 HTTP 标头
        /// </summary>
        public override NameValueCollection Headers
        {
            get
            {
                return _context.Request.Headers;
            }
        }

        /// <summary>
        /// 获取握手请求中包含的 Host 标头的值
        /// </summary>
        public override string Host
        {
            get
            {
                return _context.Request.UserHostName;
            }
        }

        /// <summary>
        /// 获取指示客户端是否通过身份验证的值
        /// </summary>
        public override bool IsAuthenticated
        {
            get
            {
                return _context.Request.IsAuthenticated;
            }
        }

        /// <summary>
        /// 获取一个值，该值指示握手请求是否从本地计算机发送
        /// </summary>
        public override bool IsLocal
        {
            get
            {
                return _context.Request.IsLocal;
            }
        }

        /// <summary>
        /// 获取一个值，该值指示是否使用安全连接来发送握手请求
        /// </summary>
        public override bool IsSecureConnection
        {
            get
            {
                return _context.Request.IsSecureConnection;
            }
        }

        /// <summary>
        /// 获取一个值，该值指示该请求是否为 WebSocket 握手请求
        /// </summary>
        public override bool IsWebSocketRequest
        {
            get
            {
                return _context.Request.IsWebSocketRequest;
            }
        }

        /// <summary>
        /// 获取握手请求中包含的 Origin 标头的值
        /// </summary>
        public override string Origin
        {
            get
            {
                return _context.Request.Headers["Origin"];
            }
        }

        /// <summary>
        /// 获取握手请求中包含的查询字符串
        /// </summary>
        public override NameValueCollection QueryString
        {
            get
            {
                return _context.Request.QueryString;
            }
        }

        /// <summary>
        /// 获取客户端请求的URI
        /// </summary>
        public override Uri RequestUri
        {
            get
            {
                return _context.Request.Url;
            }
        }

        /// <summary>
        /// 获取握手请求中包含的 Sec-WebSocket-Key 头的值
        /// </summary>
        public override string SecWebSocketKey
        {
            get
            {
                return _context.Request.Headers["Sec-WebSocket-Key"];
            }
        }

        /// <summary>
        /// 从握手请求中包含的 Sec-WebSocket-Protocol 标头中获取子协议的名称
        /// </summary>
        public override IEnumerable<string> SecWebSocketProtocols
        {
            get
            {
                var val = _context.Request.Headers["Sec-WebSocket-Protocol"];
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
        /// 获取握手请求中包含的 Sec-WebSocket-Version 头的值
        /// </summary>
        public override string SecWebSocketVersion
        {
            get
            {
                return _context.Request.Headers["Sec-WebSocket-Version"];
            }
        }

        /// <summary>
        /// 获取握手请求发送到的端点
        /// </summary>
        public override System.Net.IPEndPoint ServerEndPoint
        {
            get
            {
                return _context.Request.LocalEndPoint;
            }
        }

        /// <summary>
        /// 获取客户端信息
        /// </summary>
        public override IPrincipal User
        {
            get
            {
                return _context.User;
            }
        }

        /// <summary>
        /// 获取发送握手请求的端点
        /// </summary>
        public override System.Net.IPEndPoint UserEndPoint
        {
            get
            {
                return _context.Request.RemoteEndPoint;
            }
        }

        /// <summary>
        /// 获取用于客户端和服务器之间双向通信的 WebSocket 实例
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
        internal void Close()
        {
            _context.Connection.Close(true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="code"></param>
        internal void Close(HttpStatusCode code)
        {
            _context.Response.StatusCode = (int)code;
            _context.Response.Close();
        }

        /// <summary>
        /// 返回代表当前实例的字符串
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return _context.Request.ToString();
        }
    }
}
