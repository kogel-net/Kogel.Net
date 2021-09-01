using Kogel.Net.WebSocket.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace Kogel.Net.WebSocket.Extension.Net
{
    /// <summary>
    /// 表示对 <see cref="HttpListener"/> 实例收到的 HTTP 请求的 HTTP 响应
    /// </summary>
    public sealed class HttpListenerResponse : IDisposable
    {
        private bool _closeConnection;
        private Encoding _contentEncoding;
        private long _contentLength;
        private string _contentType;
        private HttpListenerContext _context;
        private CookieCollection _cookies;
        private bool _disposed;
        private WebHeaderCollection _headers;
        private bool _headersSent;
        private bool _keepAlive;
        private ResponseStream _outputStream;
        private Uri _redirectLocation;
        private bool _sendChunked;
        private int _statusCode;
        private string _statusDescription;
        private Version _version;

        internal HttpListenerResponse(HttpListenerContext context)
        {
            _context = context;
            _keepAlive = true;
            _statusCode = 200;
            _statusDescription = "OK";
            _version = HttpVersion.Version11;
        }

        internal bool CloseConnection
        {
            get
            {
                return _closeConnection;
            }

            set
            {
                _closeConnection = value;
            }
        }

        internal WebHeaderCollection FullHeaders
        {
            get
            {
                var headers = new WebHeaderCollection(HttpHeaderType.Response, true);

                if (_headers != null)
                    headers.Add(_headers);

                if (_contentType != null)
                {
                    headers.InternalSet(
                      "Content-Type",
                      createContentTypeHeaderText(_contentType, _contentEncoding),
                      true
                    );
                }

                if (headers["Server"] == null)
                    headers.InternalSet("Server", "websocket-sharp/1.0", true);

                if (headers["Date"] == null)
                {
                    headers.InternalSet(
                      "Date",
                      DateTime.UtcNow.ToString("r", CultureInfo.InvariantCulture),
                      true
                    );
                }

                if (_sendChunked)
                {
                    headers.InternalSet("Transfer-Encoding", "chunked", true);
                }
                else
                {
                    headers.InternalSet(
                      "Content-Length",
                      _contentLength.ToString(CultureInfo.InvariantCulture),
                      true
                    );
                }

                /*
                 * Apache forces closing the connection for these status codes:
                 * - 400 Bad Request
                 * - 408 Request Timeout
                 * - 411 Length Required
                 * - 413 Request Entity Too Large
                 * - 414 Request-Uri Too Long
                 * - 500 Internal Server Error
                 * - 503 Service Unavailable
                 */
                var closeConn = !_context.Request.KeepAlive
                                || !_keepAlive
                                || _statusCode == 400
                                || _statusCode == 408
                                || _statusCode == 411
                                || _statusCode == 413
                                || _statusCode == 414
                                || _statusCode == 500
                                || _statusCode == 503;

                var reuses = _context.Connection.Reuses;

                if (closeConn || reuses >= 100)
                {
                    headers.InternalSet("Connection", "close", true);
                }
                else
                {
                    headers.InternalSet(
                      "Keep-Alive",
                      String.Format("timeout=15,max={0}", 100 - reuses),
                      true
                    );

                    if (_context.Request.ProtocolVersion < HttpVersion.Version11)
                        headers.InternalSet("Connection", "keep-alive", true);
                }

                if (_redirectLocation != null)
                    headers.InternalSet("Location", _redirectLocation.AbsoluteUri, true);

                if (_cookies != null)
                {
                    foreach (var cookie in _cookies)
                    {
                        headers.InternalSet(
                          "Set-Cookie",
                          cookie.ToResponseString(),
                          true
                        );
                    }
                }

                return headers;
            }
        }

        internal bool HeadersSent
        {
            get
            {
                return _headersSent;
            }

            set
            {
                _headersSent = value;
            }
        }

        internal string StatusLine
        {
            get
            {
                return String.Format(
                         "HTTP/{0} {1} {2}\r\n",
                         _version,
                         _statusCode,
                         _statusDescription
                       );
            }
        }

/// <summary>
/// 
/// </summary>
        public Encoding ContentEncoding
        {
            get
            {
                return _contentEncoding;
            }

            set
            {
                if (_disposed)
                {
                    var name = GetType().ToString();
                    throw new ObjectDisposedException(name);
                }

                if (_headersSent)
                {
                    var msg = "The response is already being sent.";
                    throw new InvalidOperationException(msg);
                }

                _contentEncoding = value;
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

            set
            {
                if (_disposed)
                {
                    var name = GetType().ToString();
                    throw new ObjectDisposedException(name);
                }

                if (_headersSent)
                {
                    var msg = "The response is already being sent.";
                    throw new InvalidOperationException(msg);
                }

                if (value < 0)
                {
                    var msg = "Less than zero.";
                    throw new ArgumentOutOfRangeException(msg, "value");
                }

                _contentLength = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string ContentType
        {
            get
            {
                return _contentType;
            }

            set
            {
                if (_disposed)
                {
                    var name = GetType().ToString();
                    throw new ObjectDisposedException(name);
                }

                if (_headersSent)
                {
                    var msg = "The response is already being sent.";
                    throw new InvalidOperationException(msg);
                }

                if (value == null)
                {
                    _contentType = null;
                    return;
                }

                if (value.Length == 0)
                {
                    var msg = "An empty string.";
                    throw new ArgumentException(msg, "value");
                }

                if (!isValidForContentType(value))
                {
                    var msg = "It contains an invalid character.";
                    throw new ArgumentException(msg, "value");
                }

                _contentType = value;
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
                    _cookies = new CookieCollection();

                return _cookies;
            }

            set
            {
                _cookies = value;
            }
        }

/// <summary>
/// 
/// </summary>
        public WebHeaderCollection Headers
        {
            get
            {
                if (_headers == null)
                    _headers = new WebHeaderCollection(HttpHeaderType.Response, false);

                return _headers;
            }

            set
            {
                if (value == null)
                {
                    _headers = null;
                    return;
                }

                if (value.State != HttpHeaderType.Response)
                {
                    var msg = "The value is not valid for a response.";
                    throw new InvalidOperationException(msg);
                }

                _headers = value;
            }
        }

    /// <summary>
    /// 
    /// </summary>
        public bool KeepAlive
        {
            get
            {
                return _keepAlive;
            }

            set
            {
                if (_disposed)
                {
                    var name = GetType().ToString();
                    throw new ObjectDisposedException(name);
                }

                if (_headersSent)
                {
                    var msg = "The response is already being sent.";
                    throw new InvalidOperationException(msg);
                }

                _keepAlive = value;
            }
        }

     /// <summary>
     /// 
     /// </summary>
        public Stream OutputStream
        {
            get
            {
                if (_disposed)
                {
                    var name = GetType().ToString();
                    throw new ObjectDisposedException(name);
                }

                if (_outputStream == null)
                    _outputStream = _context.Connection.GetResponseStream();

                return _outputStream;
            }
        }

      /// <summary>
      /// 
      /// </summary>
        public Version ProtocolVersion
        {
            get
            {
                return _version;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string RedirectLocation
        {
            get
            {
                return _redirectLocation != null
                       ? _redirectLocation.OriginalString
                       : null;
            }

            set
            {
                if (_disposed)
                {
                    var name = GetType().ToString();
                    throw new ObjectDisposedException(name);
                }

                if (_headersSent)
                {
                    var msg = "The response is already being sent.";
                    throw new InvalidOperationException(msg);
                }

                if (value == null)
                {
                    _redirectLocation = null;
                    return;
                }

                if (value.Length == 0)
                {
                    var msg = "An empty string.";
                    throw new ArgumentException(msg, "value");
                }

                Uri uri;
                if (!Uri.TryCreate(value, UriKind.Absolute, out uri))
                {
                    var msg = "Not an absolute URL.";
                    throw new ArgumentException(msg, "value");
                }

                _redirectLocation = uri;
            }
        }

/// <summary>
/// 
/// </summary>
        public bool SendChunked
        {
            get
            {
                return _sendChunked;
            }

            set
            {
                if (_disposed)
                {
                    var name = GetType().ToString();
                    throw new ObjectDisposedException(name);
                }

                if (_headersSent)
                {
                    var msg = "The response is already being sent.";
                    throw new InvalidOperationException(msg);
                }

                _sendChunked = value;
            }
        }

       /// <summary>
       /// 
       /// </summary>
        public int StatusCode
        {
            get
            {
                return _statusCode;
            }

            set
            {
                if (_disposed)
                {
                    var name = GetType().ToString();
                    throw new ObjectDisposedException(name);
                }

                if (_headersSent)
                {
                    var msg = "The response is already being sent.";
                    throw new InvalidOperationException(msg);
                }

                if (value < 100 || value > 999)
                {
                    var msg = "A value is not between 100 and 999 inclusive.";
                    throw new System.Net.ProtocolViolationException(msg);
                }

                _statusCode = value;
                _statusDescription = value.GetStatusDescription();
            }
        }

       /// <summary>
       /// 
       /// </summary>
        public string StatusDescription
        {
            get
            {
                return _statusDescription;
            }

            set
            {
                if (_disposed)
                {
                    var name = GetType().ToString();
                    throw new ObjectDisposedException(name);
                }

                if (_headersSent)
                {
                    var msg = "The response is already being sent.";
                    throw new InvalidOperationException(msg);
                }

                if (value == null)
                    throw new ArgumentNullException("value");

                if (value.Length == 0)
                {
                    _statusDescription = _statusCode.GetStatusDescription();
                    return;
                }

                if (!isValidForStatusDescription(value))
                {
                    var msg = "It contains an invalid character.";
                    throw new ArgumentException(msg, "value");
                }

                _statusDescription = value;
            }
        }

        private bool canSetCookie(Cookie cookie)
        {
            var found = findCookie(cookie).ToList();

            if (found.Count == 0)
                return true;

            var ver = cookie.Version;

            foreach (var c in found)
            {
                if (c.Version == ver)
                    return true;
            }

            return false;
        }

        private void close(bool force)
        {
            _disposed = true;
            _context.Connection.Close(force);
        }

        private void close(byte[] responseEntity, int bufferLength, bool willBlock)
        {
            var stream = OutputStream;

            if (willBlock)
            {
                stream.WriteBytes(responseEntity, bufferLength);
                close(false);

                return;
            }

            stream.WriteBytesAsync(
              responseEntity,
              bufferLength,
              () => close(false),
              null
            );
        }

        private static string createContentTypeHeaderText(
          string value, Encoding encoding
        )
        {
            if (value.IndexOf("charset=", StringComparison.Ordinal) > -1)
                return value;

            if (encoding == null)
                return value;

            return String.Format("{0}; charset={1}", value, encoding.WebName);
        }

        private IEnumerable<Cookie> findCookie(Cookie cookie)
        {
            if (_cookies == null || _cookies.Count == 0)
                yield break;

            foreach (var c in _cookies)
            {
                if (c.EqualsWithoutValueAndVersion(cookie))
                    yield return c;
            }
        }

        private static bool isValidForContentType(string value)
        {
            foreach (var c in value)
            {
                if (c < 0x20)
                    return false;

                if (c > 0x7e)
                    return false;

                if ("()<>@:\\[]?{}".IndexOf(c) > -1)
                    return false;
            }

            return true;
        }

        private static bool isValidForStatusDescription(string value)
        {
            foreach (var c in value)
            {
                if (c < 0x20)
                    return false;

                if (c > 0x7e)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Abort()
        {
            if (_disposed)
                return;

            close(true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cookie"></param>
        public void AppendCookie(Cookie cookie)
        {
            Cookies.Add(cookie);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void AppendHeader(string name, string value)
        {
            Headers.Add(name, value);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Close()
        {
            if (_disposed)
                return;

            close(false);
        }

        /// <summary>
        /// 向客户端发送带有指定实体主体数据的响应并释放此实例使用的资源
        /// </summary>
        /// <param name="responseEntity"></param>
        /// <param name="willBlock"></param>
        public void Close(byte[] responseEntity, bool willBlock)
        {
            if (_disposed)
            {
                var name = GetType().ToString();
                throw new ObjectDisposedException(name);
            }

            if (responseEntity == null)
                throw new ArgumentNullException("responseEntity");

            var len = responseEntity.LongLength;

            if (len > Int32.MaxValue)
            {
                close(responseEntity, 1024, willBlock);
                return;
            }

            var stream = OutputStream;

            if (willBlock)
            {
                stream.Write(responseEntity, 0, (int)len);
                close(false);

                return;
            }

            stream.BeginWrite(
              responseEntity,
              0,
              (int)len,
              ar =>
              {
                  stream.EndWrite(ar);
                  close(false);
              },
              null
            );
        }

        /// <summary>
        /// 将一些属性从指定的响应实例复制到这个实例
        /// </summary>
        /// <param name="templateResponse"></param>
        public void CopyFrom(HttpListenerResponse templateResponse)
        {
            if (templateResponse == null)
                throw new ArgumentNullException("templateResponse");

            var headers = templateResponse._headers;

            if (headers != null)
            {
                if (_headers != null)
                    _headers.Clear();

                Headers.Add(headers);
            }
            else
            {
                _headers = null;
            }

            _contentLength = templateResponse._contentLength;
            _statusCode = templateResponse._statusCode;
            _statusDescription = templateResponse._statusDescription;
            _keepAlive = templateResponse._keepAlive;
            _version = templateResponse._version;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        public void Redirect(string url)
        {
            if (_disposed)
            {
                var name = GetType().ToString();
                throw new ObjectDisposedException(name);
            }

            if (_headersSent)
            {
                var msg = "The response is already being sent.";
                throw new InvalidOperationException(msg);
            }

            if (url == null)
                throw new ArgumentNullException("url");

            if (url.Length == 0)
            {
                var msg = "An empty string.";
                throw new ArgumentException(msg, "url");
            }

            Uri uri;
            if (!Uri.TryCreate(url, UriKind.Absolute, out uri))
            {
                var msg = "Not an absolute URL.";
                throw new ArgumentException(msg, "url");
            }

            _redirectLocation = uri;
            _statusCode = 302;
            _statusDescription = "Found";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cookie"></param>
        public void SetCookie(Cookie cookie)
        {
            if (cookie == null)
                throw new ArgumentNullException("cookie");

            if (!canSetCookie(cookie))
            {
                var msg = "It cannot be updated.";
                throw new ArgumentException(msg, "cookie");
            }

            Cookies.Add(cookie);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void SetHeader(string name, string value)
        {
            Headers.Set(name, value);
        }

        /// <summary>
        /// 
        /// </summary>
        void IDisposable.Dispose()
        {
            if (_disposed)
                return;

            close(true);
        }
    }
}
