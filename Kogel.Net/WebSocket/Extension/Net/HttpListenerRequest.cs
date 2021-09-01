using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Kogel.Net.WebSocket.Extension.Net
{
    /// <summary>
    /// 表示对 <see cref="HttpListener"/> 实例的传入 HTTP 请求
    /// </summary>
    public sealed class HttpListenerRequest
    {
        private static readonly byte[] _100continue;
        private string[] _acceptTypes;
        private bool _chunked;
        private HttpConnection _connection;
        private Encoding _contentEncoding;
        private long _contentLength;
        private HttpListenerContext _context;
        private CookieCollection _cookies;
        private WebHeaderCollection _headers;
        private string _httpMethod;
        private Stream _inputStream;
        private Version _protocolVersion;
        private NameValueCollection _queryString;
        private string _rawUrl;
        private Guid _requestTraceIdentifier;
        private Uri _url;
        private Uri _urlReferrer;
        private bool _urlSet;
        private string _userHostName;
        private string[] _userLanguages;

        static HttpListenerRequest()
        {
            _100continue = Encoding.ASCII.GetBytes("HTTP/1.1 100 Continue\r\n\r\n");
        }

        internal HttpListenerRequest(HttpListenerContext context)
        {
            _context = context;

            _connection = context.Connection;
            _contentLength = -1;
            _headers = new WebHeaderCollection();
            _requestTraceIdentifier = Guid.NewGuid();
        }

        /// <summary>
        /// 获取客户端可接受的媒体类型
        /// </summary>
        public string[] AcceptTypes
        {
            get
            {
                var val = _headers["Accept"];

                if (val == null)
                    return null;

                if (_acceptTypes == null)
                {
                    _acceptTypes = val
                                   .SplitHeaderValue(',')
                                   .TrimEach()
                                   .ToList()
                                   .ToArray();
                }

                return _acceptTypes;
            }
        }

        /// <summary>
        /// 获取标识客户端提供的证书有问题的错误代码
        /// </summary>
        public int ClientCertificateError
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public Encoding ContentEncoding
        {
            get
            {
                if (_contentEncoding == null)
                    _contentEncoding = getContentEncoding();

                return _contentEncoding;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public long ContentLength64
        {
            get
            {
                return _contentLength;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string ContentType
        {
            get
            {
                return _headers["Content-Type"];
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public CookieCollection Cookies
        {
            get
            {
                if (_cookies == null)
                    _cookies = _headers.GetCookies(false);

                return _cookies;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool HasEntityBody
        {
            get
            {
                return _contentLength > 0 || _chunked;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public NameValueCollection Headers
        {
            get
            {
                return _headers;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string HttpMethod
        {
            get
            {
                return _httpMethod;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public Stream InputStream
        {
            get
            {
                if (_inputStream == null)
                {
                    _inputStream = _contentLength > 0 || _chunked
                                   ? _connection
                                     .GetRequestStream(_contentLength, _chunked)
                                   : Stream.Null;
                }

                return _inputStream;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool IsAuthenticated
        {
            get
            {
                return _context.User != null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool IsLocal
        {
            get
            {
                return _connection.IsLocal;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool IsSecureConnection
        {
            get
            {
                return _connection.IsSecure;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool IsWebSocketRequest
        {
            get
            {
                return _httpMethod == "GET" && _headers.Upgrades("websocket");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool KeepAlive
        {
            get
            {
                return _headers.KeepsAlive(_protocolVersion);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public System.Net.IPEndPoint LocalEndPoint
        {
            get
            {
                return _connection.LocalEndPoint;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public Version ProtocolVersion
        {
            get
            {
                return _protocolVersion;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public NameValueCollection QueryString
        {
            get
            {
                if (_queryString == null)
                {
                    var url = Url;

                    _queryString = QueryStringCollection
                                   .Parse(
                                     url != null ? url.Query : null,
                                     Encoding.UTF8
                                   );
                }

                return _queryString;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string RawUrl
        {
            get
            {
                return _rawUrl;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public System.Net.IPEndPoint RemoteEndPoint
        {
            get
            {
                return _connection.RemoteEndPoint;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public Guid RequestTraceIdentifier
        {
            get
            {
                return _requestTraceIdentifier;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public Uri Url
        {
            get
            {
                if (!_urlSet)
                {
                    _url = HttpUtility
                           .CreateRequestUrl(
                             _rawUrl,
                             _userHostName ?? UserHostAddress,
                             IsWebSocketRequest,
                             IsSecureConnection
                           );

                    _urlSet = true;
                }

                return _url;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public Uri UrlReferrer
        {
            get
            {
                var val = _headers["Referer"];

                if (val == null)
                    return null;

                if (_urlReferrer == null)
                    _urlReferrer = val.ToUri();

                return _urlReferrer;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string UserAgent
        {
            get
            {
                return _headers["User-Agent"];
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string UserHostAddress
        {
            get
            {
                return _connection.LocalEndPoint.ToString();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string UserHostName
        {
            get
            {
                return _userHostName;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string[] UserLanguages
        {
            get
            {
                var val = _headers["Accept-Language"];

                if (val == null)
                    return null;

                if (_userLanguages == null)
                    _userLanguages = val.Split(',').TrimEach().ToList().ToArray();

                return _userLanguages;
            }
        }

        private Encoding getContentEncoding()
        {
            var val = _headers["Content-Type"];

            if (val == null)
                return Encoding.UTF8;

            Encoding ret;

            return HttpUtility.TryGetEncoding(val, out ret)
                   ? ret
                   : Encoding.UTF8;
        }

        internal void AddHeader(string headerField)
        {
            var start = headerField[0];

            if (start == ' ' || start == '\t')
            {
                _context.ErrorMessage = "Invalid header field";

                return;
            }

            var colon = headerField.IndexOf(':');

            if (colon < 1)
            {
                _context.ErrorMessage = "Invalid header field";

                return;
            }

            var name = headerField.Substring(0, colon).Trim();

            if (name.Length == 0 || !name.IsToken())
            {
                _context.ErrorMessage = "Invalid header name";

                return;
            }

            var val = colon < headerField.Length - 1
                      ? headerField.Substring(colon + 1).Trim()
                      : String.Empty;

            _headers.InternalSet(name, val, false);

            var lower = name.ToLower(CultureInfo.InvariantCulture);

            if (lower == "host")
            {
                if (_userHostName != null)
                {
                    _context.ErrorMessage = "Invalid Host header";

                    return;
                }

                if (val.Length == 0)
                {
                    _context.ErrorMessage = "Invalid Host header";

                    return;
                }

                _userHostName = val;

                return;
            }

            if (lower == "content-length")
            {
                if (_contentLength > -1)
                {
                    _context.ErrorMessage = "Invalid Content-Length header";

                    return;
                }

                long len;

                if (!Int64.TryParse(val, out len))
                {
                    _context.ErrorMessage = "Invalid Content-Length header";

                    return;
                }

                if (len < 0)
                {
                    _context.ErrorMessage = "Invalid Content-Length header";

                    return;
                }

                _contentLength = len;

                return;
            }
        }

        internal void FinishInitialization()
        {
            if (_userHostName == null)
            {
                _context.ErrorMessage = "Host header required";

                return;
            }

            var transferEnc = _headers["Transfer-Encoding"];

            if (transferEnc != null)
            {
                var comparison = StringComparison.OrdinalIgnoreCase;

                if (!transferEnc.Equals("chunked", comparison))
                {
                    _context.ErrorMessage = "Invalid Transfer-Encoding header";
                    _context.ErrorStatusCode = 501;

                    return;
                }

                _chunked = true;
            }

            if (_httpMethod == "POST" || _httpMethod == "PUT")
            {
                if (_contentLength <= 0 && !_chunked)
                {
                    _context.ErrorMessage = String.Empty;
                    _context.ErrorStatusCode = 411;

                    return;
                }
            }

            var expect = _headers["Expect"];

            if (expect != null)
            {
                var comparison = StringComparison.OrdinalIgnoreCase;

                if (!expect.Equals("100-continue", comparison))
                {
                    _context.ErrorMessage = "Invalid Expect header";

                    return;
                }

                var output = _connection.GetResponseStream();
                output.InternalWrite(_100continue, 0, _100continue.Length);
            }
        }

        internal bool FlushInput()
        {
            var input = InputStream;

            if (input == Stream.Null)
                return true;

            var len = 2048;

            if (_contentLength > 0 && _contentLength < len)
                len = (int)_contentLength;

            var buff = new byte[len];

            while (true)
            {
                try
                {
                    var ares = input.BeginRead(buff, 0, len, null, null);

                    if (!ares.IsCompleted)
                    {
                        var timeout = 100;

                        if (!ares.AsyncWaitHandle.WaitOne(timeout))
                            return false;
                    }

                    if (input.EndRead(ares) <= 0)
                        return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        internal bool IsUpgradeRequest(string protocol)
        {
            return _headers.Upgrades(protocol);
        }

        internal void SetRequestLine(string requestLine)
        {
            var parts = requestLine.Split(new[] { ' ' }, 3);

            if (parts.Length < 3)
            {
                _context.ErrorMessage = "Invalid request line (parts)";

                return;
            }

            var method = parts[0];

            if (method.Length == 0)
            {
                _context.ErrorMessage = "Invalid request line (method)";

                return;
            }

            var target = parts[1];

            if (target.Length == 0)
            {
                _context.ErrorMessage = "Invalid request line (target)";

                return;
            }

            var rawVer = parts[2];

            if (rawVer.Length != 8)
            {
                _context.ErrorMessage = "Invalid request line (version)";

                return;
            }

            if (!rawVer.StartsWith("HTTP/", StringComparison.Ordinal))
            {
                _context.ErrorMessage = "Invalid request line (version)";

                return;
            }

            Version ver;

            if (!rawVer.Substring(5).TryCreateVersion(out ver))
            {
                _context.ErrorMessage = "Invalid request line (version)";

                return;
            }

            if (ver != HttpVersion.Version11)
            {
                _context.ErrorMessage = "Invalid request line (version)";
                _context.ErrorStatusCode = 505;

                return;
            }

            if (!method.IsHttpMethod(ver))
            {
                _context.ErrorMessage = "Invalid request line (method)";
                _context.ErrorStatusCode = 501;

                return;
            }

            _httpMethod = method;
            _rawUrl = target;
            _protocolVersion = ver;
        }

        /// <summary>
        /// 开始异步获取客户端提供的证书
        /// </summary>
        /// <param name="requestCallback"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public IAsyncResult BeginGetClientCertificate(
          AsyncCallback requestCallback, object state
        )
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// 结束异步操作，获取客户端提供的证书
        /// </summary>
        /// <param name="asyncResult"></param>
        /// <returns></returns>
        public X509Certificate2 EndGetClientCertificate(IAsyncResult asyncResult)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// 获取客户端提供的证书
        /// </summary>
        /// <returns></returns>
        public X509Certificate2 GetClientCertificate()
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var buff = new StringBuilder(64);

            buff
            .AppendFormat(
              "{0} {1} HTTP/{2}\r\n", _httpMethod, _rawUrl, _protocolVersion
            )
            .Append(_headers.ToString());

            return buff.ToString();
        }
    }
}
