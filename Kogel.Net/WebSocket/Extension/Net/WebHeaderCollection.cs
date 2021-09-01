using Kogel.Net.WebSocket.Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;

namespace Kogel.Net.WebSocket.Extension.Net
{
    /// <summary>
    /// 提供与请求或响应关联的 HTTP 标头的集合
    /// </summary>
    [Serializable]
    [ComVisible(true)]
    public class WebHeaderCollection : NameValueCollection, ISerializable
    {
        private static readonly Dictionary<string, HttpHeaderInfo> _headers;
        private bool _internallyUsed;
        private HttpHeaderType _state;

        static WebHeaderCollection()
        {
            _headers =
              new Dictionary<string, HttpHeaderInfo>(
                StringComparer.InvariantCultureIgnoreCase
              )
              {
          {
            "Accept",
            new HttpHeaderInfo (
              "Accept",
              HttpHeaderType.Request
              | HttpHeaderType.Restricted
              | HttpHeaderType.MultiValue
            )
          },
          {
            "AcceptCharset",
            new HttpHeaderInfo (
              "Accept-Charset",
              HttpHeaderType.Request | HttpHeaderType.MultiValue
            )
          },
          {
            "AcceptEncoding",
            new HttpHeaderInfo (
              "Accept-Encoding",
              HttpHeaderType.Request | HttpHeaderType.MultiValue
            )
          },
          {
            "AcceptLanguage",
            new HttpHeaderInfo (
              "Accept-Language",
              HttpHeaderType.Request | HttpHeaderType.MultiValue
            )
          },
          {
            "AcceptRanges",
            new HttpHeaderInfo (
              "Accept-Ranges",
              HttpHeaderType.Response | HttpHeaderType.MultiValue
            )
          },
          {
            "Age",
            new HttpHeaderInfo (
              "Age",
              HttpHeaderType.Response
            )
          },
          {
            "Allow",
            new HttpHeaderInfo (
              "Allow",
              HttpHeaderType.Request
              | HttpHeaderType.Response
              | HttpHeaderType.MultiValue
            )
          },
          {
            "Authorization",
            new HttpHeaderInfo (
              "Authorization",
              HttpHeaderType.Request | HttpHeaderType.MultiValue
            )
          },
          {
            "CacheControl",
            new HttpHeaderInfo (
              "Cache-Control",
              HttpHeaderType.Request
              | HttpHeaderType.Response
              | HttpHeaderType.MultiValue
            )
          },
          {
            "Connection",
            new HttpHeaderInfo (
              "Connection",
              HttpHeaderType.Request
              | HttpHeaderType.Response
              | HttpHeaderType.Restricted
              | HttpHeaderType.MultiValue
            )
          },
          {
            "ContentEncoding",
            new HttpHeaderInfo (
              "Content-Encoding",
              HttpHeaderType.Request
              | HttpHeaderType.Response
              | HttpHeaderType.MultiValue
            )
          },
          {
            "ContentLanguage",
            new HttpHeaderInfo (
              "Content-Language",
              HttpHeaderType.Request
              | HttpHeaderType.Response
              | HttpHeaderType.MultiValue
            )
          },
          {
            "ContentLength",
            new HttpHeaderInfo (
              "Content-Length",
              HttpHeaderType.Request
              | HttpHeaderType.Response
              | HttpHeaderType.Restricted
            )
          },
          {
            "ContentLocation",
            new HttpHeaderInfo (
              "Content-Location",
              HttpHeaderType.Request | HttpHeaderType.Response
            )
          },
          {
            "ContentMd5",
            new HttpHeaderInfo (
              "Content-MD5",
              HttpHeaderType.Request | HttpHeaderType.Response
            )
          },
          {
            "ContentRange",
            new HttpHeaderInfo (
              "Content-Range",
              HttpHeaderType.Request | HttpHeaderType.Response
            )
          },
          {
            "ContentType",
            new HttpHeaderInfo (
              "Content-Type",
              HttpHeaderType.Request
              | HttpHeaderType.Response
              | HttpHeaderType.Restricted
            )
          },
          {
            "Cookie",
            new HttpHeaderInfo (
              "Cookie",
              HttpHeaderType.Request
            )
          },
          {
            "Cookie2",
            new HttpHeaderInfo (
              "Cookie2",
              HttpHeaderType.Request
            )
          },
          {
            "Date",
            new HttpHeaderInfo (
              "Date",
              HttpHeaderType.Request
              | HttpHeaderType.Response
              | HttpHeaderType.Restricted
            )
          },
          {
            "Expect",
            new HttpHeaderInfo (
              "Expect",
              HttpHeaderType.Request
              | HttpHeaderType.Restricted
              | HttpHeaderType.MultiValue
            )
          },
          {
            "Expires",
            new HttpHeaderInfo (
              "Expires",
              HttpHeaderType.Request | HttpHeaderType.Response
            )
          },
          {
            "ETag",
            new HttpHeaderInfo (
              "ETag",
              HttpHeaderType.Response
            )
          },
          {
            "From",
            new HttpHeaderInfo (
              "From",
              HttpHeaderType.Request
            )
          },
          {
            "Host",
            new HttpHeaderInfo (
              "Host",
              HttpHeaderType.Request | HttpHeaderType.Restricted
            )
          },
          {
            "IfMatch",
            new HttpHeaderInfo (
              "If-Match",
              HttpHeaderType.Request | HttpHeaderType.MultiValue
            )
          },
          {
            "IfModifiedSince",
            new HttpHeaderInfo (
              "If-Modified-Since",
              HttpHeaderType.Request | HttpHeaderType.Restricted
            )
          },
          {
            "IfNoneMatch",
            new HttpHeaderInfo (
              "If-None-Match",
              HttpHeaderType.Request | HttpHeaderType.MultiValue
            )
          },
          {
            "IfRange",
            new HttpHeaderInfo (
              "If-Range",
              HttpHeaderType.Request
            )
          },
          {
            "IfUnmodifiedSince",
            new HttpHeaderInfo (
              "If-Unmodified-Since",
              HttpHeaderType.Request
            )
          },
          {
            "KeepAlive",
            new HttpHeaderInfo (
              "Keep-Alive",
              HttpHeaderType.Request
              | HttpHeaderType.Response
              | HttpHeaderType.MultiValue
            )
          },
          {
            "LastModified",
            new HttpHeaderInfo (
              "Last-Modified",
              HttpHeaderType.Request | HttpHeaderType.Response
            )
          },
          {
            "Location",
            new HttpHeaderInfo (
              "Location",
              HttpHeaderType.Response
            )
          },
          {
            "MaxForwards",
            new HttpHeaderInfo (
              "Max-Forwards",
              HttpHeaderType.Request
            )
          },
          {
            "Pragma",
            new HttpHeaderInfo (
              "Pragma",
              HttpHeaderType.Request | HttpHeaderType.Response
            )
          },
          {
            "ProxyAuthenticate",
            new HttpHeaderInfo (
              "Proxy-Authenticate",
              HttpHeaderType.Response | HttpHeaderType.MultiValue
            )
          },
          {
            "ProxyAuthorization",
            new HttpHeaderInfo (
              "Proxy-Authorization",
              HttpHeaderType.Request
            )
          },
          {
            "ProxyConnection",
            new HttpHeaderInfo (
              "Proxy-Connection",
              HttpHeaderType.Request
              | HttpHeaderType.Response
              | HttpHeaderType.Restricted
            )
          },
          {
            "Public",
            new HttpHeaderInfo (
              "Public",
              HttpHeaderType.Response | HttpHeaderType.MultiValue
            )
          },
          {
            "Range",
            new HttpHeaderInfo (
              "Range",
              HttpHeaderType.Request
              | HttpHeaderType.Restricted
              | HttpHeaderType.MultiValue
            )
          },
          {
            "Referer",
            new HttpHeaderInfo (
              "Referer",
              HttpHeaderType.Request | HttpHeaderType.Restricted
            )
          },
          {
            "RetryAfter",
            new HttpHeaderInfo (
              "Retry-After",
              HttpHeaderType.Response
            )
          },
          {
            "SecWebSocketAccept",
            new HttpHeaderInfo (
              "Sec-WebSocket-Accept",
              HttpHeaderType.Response | HttpHeaderType.Restricted
            )
          },
          {
            "SecWebSocketExtensions",
            new HttpHeaderInfo (
              "Sec-WebSocket-Extensions",
              HttpHeaderType.Request
              | HttpHeaderType.Response
              | HttpHeaderType.Restricted
              | HttpHeaderType.MultiValueInRequest
            )
          },
          {
            "SecWebSocketKey",
            new HttpHeaderInfo (
              "Sec-WebSocket-Key",
              HttpHeaderType.Request | HttpHeaderType.Restricted
            )
          },
          {
            "SecWebSocketProtocol",
            new HttpHeaderInfo (
              "Sec-WebSocket-Protocol",
              HttpHeaderType.Request
              | HttpHeaderType.Response
              | HttpHeaderType.MultiValueInRequest
            )
          },
          {
            "SecWebSocketVersion",
            new HttpHeaderInfo (
              "Sec-WebSocket-Version",
              HttpHeaderType.Request
              | HttpHeaderType.Response
              | HttpHeaderType.Restricted
              | HttpHeaderType.MultiValueInResponse
            )
          },
          {
            "Server",
            new HttpHeaderInfo (
              "Server",
              HttpHeaderType.Response
            )
          },
          {
            "SetCookie",
            new HttpHeaderInfo (
              "Set-Cookie",
              HttpHeaderType.Response | HttpHeaderType.MultiValue
            )
          },
          {
            "SetCookie2",
            new HttpHeaderInfo (
              "Set-Cookie2",
              HttpHeaderType.Response | HttpHeaderType.MultiValue
            )
          },
          {
            "Te",
            new HttpHeaderInfo (
              "TE",
              HttpHeaderType.Request
            )
          },
          {
            "Trailer",
            new HttpHeaderInfo (
              "Trailer",
              HttpHeaderType.Request | HttpHeaderType.Response
            )
          },
          {
            "TransferEncoding",
            new HttpHeaderInfo (
              "Transfer-Encoding",
              HttpHeaderType.Request
              | HttpHeaderType.Response
              | HttpHeaderType.Restricted
              | HttpHeaderType.MultiValue
            )
          },
          {
            "Translate",
            new HttpHeaderInfo (
              "Translate",
              HttpHeaderType.Request
            )
          },
          {
            "Upgrade",
            new HttpHeaderInfo (
              "Upgrade",
              HttpHeaderType.Request
              | HttpHeaderType.Response
              | HttpHeaderType.MultiValue
            )
          },
          {
            "UserAgent",
            new HttpHeaderInfo (
              "User-Agent",
              HttpHeaderType.Request | HttpHeaderType.Restricted
            )
          },
          {
            "Vary",
            new HttpHeaderInfo (
              "Vary",
              HttpHeaderType.Response | HttpHeaderType.MultiValue
            )
          },
          {
            "Via",
            new HttpHeaderInfo (
              "Via",
              HttpHeaderType.Request
              | HttpHeaderType.Response
              | HttpHeaderType.MultiValue
            )
          },
          {
            "Warning",
            new HttpHeaderInfo (
              "Warning",
              HttpHeaderType.Request
              | HttpHeaderType.Response
              | HttpHeaderType.MultiValue
            )
          },
          {
            "WwwAuthenticate",
            new HttpHeaderInfo (
              "WWW-Authenticate",
              HttpHeaderType.Response
              | HttpHeaderType.Restricted
              | HttpHeaderType.MultiValue
            )
          }
              };
        }





        internal WebHeaderCollection(HttpHeaderType state, bool internallyUsed)
        {
            _state = state;
            _internallyUsed = internallyUsed;
        }

       /// <summary>
       /// 
       /// </summary>
       /// <param name="serializationInfo"></param>
       /// <param name="streamingContext"></param>
        protected WebHeaderCollection(
          SerializationInfo serializationInfo, StreamingContext streamingContext
        )
        {
            if (serializationInfo == null)
                throw new ArgumentNullException("serializationInfo");

            try
            {
                _internallyUsed = serializationInfo.GetBoolean("InternallyUsed");
                _state = (HttpHeaderType)serializationInfo.GetInt32("State");

                var cnt = serializationInfo.GetInt32("Count");

                for (var i = 0; i < cnt; i++)
                {
                    base.Add(
                      serializationInfo.GetString(i.ToString()),
                      serializationInfo.GetString((cnt + i).ToString())
                    );
                }
            }
            catch (SerializationException ex)
            {
                throw new ArgumentException(ex.Message, "serializationInfo", ex);
            }
        }

      /// <summary>
      /// 
      /// </summary>
        public WebHeaderCollection()
        {
        }





        internal HttpHeaderType State
        {
            get
            {
                return _state;
            }
        }


        /// <summary>
        /// 获取集合中的所有标题名称
        /// </summary>
        public override string[] AllKeys
        {
            get
            {
                return base.AllKeys;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override int Count
        {
            get
            {
                return base.Count;
            }
        }

       /// <summary>
       /// 
       /// </summary>
       /// <param name="header"></param>
       /// <returns></returns>
        public string this[HttpRequestHeader header]
        {
            get
            {
                var key = header.ToString();
                var name = getHeaderName(key);

                return Get(name);
            }

            set
            {
                Add(header, value);
            }
        }

       /// <summary>
       /// 
       /// </summary>
       /// <param name="header"></param>
       /// <returns></returns>
        public string this[HttpResponseHeader header]
        {
            get
            {
                var key = header.ToString();
                var name = getHeaderName(key);

                return Get(name);
            }

            set
            {
                Add(header, value);
            }
        }

      /// <summary>
      /// 
      /// </summary>
        public override NameObjectCollectionBase.KeysCollection Keys
        {
            get
            {
                return base.Keys;
            }
        }

        private void add(string name, string value, HttpHeaderType headerType)
        {
            base.Add(name, value);

            if (_state != HttpHeaderType.Unspecified)
                return;

            if (headerType == HttpHeaderType.Unspecified)
                return;

            _state = headerType;
        }

        private void checkAllowed(HttpHeaderType headerType)
        {
            if (_state == HttpHeaderType.Unspecified)
                return;

            if (headerType == HttpHeaderType.Unspecified)
                return;

            if (headerType != _state)
            {
                var msg = "This instance does not allow the header.";

                throw new InvalidOperationException(msg);
            }
        }

        private static string checkName(string name, string paramName)
        {
            if (name == null)
            {
                var msg = "The name is null.";

                throw new ArgumentNullException(paramName, msg);
            }

            if (name.Length == 0)
            {
                var msg = "The name is an empty string.";

                throw new ArgumentException(msg, paramName);
            }

            name = name.Trim();

            if (name.Length == 0)
            {
                var msg = "The name is a string of spaces.";

                throw new ArgumentException(msg, paramName);
            }

            if (!name.IsToken())
            {
                var msg = "The name contains an invalid character.";

                throw new ArgumentException(msg, paramName);
            }

            return name;
        }

        private void checkRestricted(string name, HttpHeaderType headerType)
        {
            if (_internallyUsed)
                return;

            var res = headerType == HttpHeaderType.Response;

            if (isRestricted(name, res))
            {
                var msg = "The header is a restricted header.";

                throw new ArgumentException(msg);
            }
        }

        private static string checkValue(string value, string paramName)
        {
            if (value == null)
                return String.Empty;

            value = value.Trim();

            var len = value.Length;

            if (len == 0)
                return value;

            if (len > 65535)
            {
                var msg = "The length of the value is greater than 65,535 characters.";

                throw new ArgumentOutOfRangeException(paramName, msg);
            }

            if (!value.IsText())
            {
                var msg = "The value contains an invalid character.";

                throw new ArgumentException(msg, paramName);
            }

            return value;
        }

        private static HttpHeaderInfo getHeaderInfo(string name)
        {
            var comparison = StringComparison.InvariantCultureIgnoreCase;

            foreach (var headerInfo in _headers.Values)
            {
                if (headerInfo.HeaderName.Equals(name, comparison))
                    return headerInfo;
            }

            return null;
        }

        private static string getHeaderName(string key)
        {
            HttpHeaderInfo headerInfo;

            return _headers.TryGetValue(key, out headerInfo)
                   ? headerInfo.HeaderName
                   : null;
        }

        private static HttpHeaderType getHeaderType(string name)
        {
            var headerInfo = getHeaderInfo(name);

            if (headerInfo == null)
                return HttpHeaderType.Unspecified;

            if (headerInfo.IsRequest)
            {
                return !headerInfo.IsResponse
                       ? HttpHeaderType.Request
                       : HttpHeaderType.Unspecified;
            }

            return headerInfo.IsResponse
                   ? HttpHeaderType.Response
                   : HttpHeaderType.Unspecified;
        }

        private static bool isMultiValue(string name, bool response)
        {
            var headerInfo = getHeaderInfo(name);

            return headerInfo != null && headerInfo.IsMultiValue(response);
        }

        private static bool isRestricted(string name, bool response)
        {
            var headerInfo = getHeaderInfo(name);

            return headerInfo != null && headerInfo.IsRestricted(response);
        }

        private void set(string name, string value, HttpHeaderType headerType)
        {
            base.Set(name, value);

            if (_state != HttpHeaderType.Unspecified)
                return;

            if (headerType == HttpHeaderType.Unspecified)
                return;

            _state = headerType;
        }

        internal void InternalRemove(string name)
        {
            base.Remove(name);
        }

        internal void InternalSet(string header, bool response)
        {
            var idx = header.IndexOf(':');

            if (idx == -1)
            {
                var msg = "It does not contain a colon character.";

                throw new ArgumentException(msg, "header");
            }

            var name = header.Substring(0, idx);
            var val = idx < header.Length - 1
                      ? header.Substring(idx + 1)
                      : String.Empty;

            name = checkName(name, "header");
            val = checkValue(val, "header");

            if (isMultiValue(name, response))
            {
                base.Add(name, val);

                return;
            }

            base.Set(name, val);
        }

        internal void InternalSet(string name, string value, bool response)
        {
            value = checkValue(value, "value");

            if (isMultiValue(name, response))
            {
                base.Add(name, value);

                return;
            }

            base.Set(name, value);
        }

        internal string ToStringMultiValue(bool response)
        {
            var cnt = Count;

            if (cnt == 0)
                return "\r\n";

            var buff = new StringBuilder();

            for (var i = 0; i < cnt; i++)
            {
                var name = GetKey(i);

                if (isMultiValue(name, response))
                {
                    foreach (var val in GetValues(i))
                        buff.AppendFormat("{0}: {1}\r\n", name, val);

                    continue;
                }

                buff.AppendFormat("{0}: {1}\r\n", name, Get(i));
            }

            buff.Append("\r\n");

            return buff.ToString();
        }

        /// <summary>
        /// 向集合添加标题而不检查标题是否在受限标题列表中
        /// </summary>
        /// <param name="headerName"></param>
        /// <param name="headerValue"></param>
        protected void AddWithoutValidate(string headerName, string headerValue)
        {
            headerName = checkName(headerName, "headerName");
            headerValue = checkValue(headerValue, "headerValue");

            var headerType = getHeaderType(headerName);

            checkAllowed(headerType);
            add(headerName, headerValue, headerType);
        }

        /// <summary>
        /// 将指定的标头添加到集合中
        /// </summary>
        /// <param name="header"></param>
        public void Add(string header)
        {
            if (header == null)
                throw new ArgumentNullException("header");

            var len = header.Length;

            if (len == 0)
            {
                var msg = "An empty string.";

                throw new ArgumentException(msg, "header");
            }

            var idx = header.IndexOf(':');

            if (idx == -1)
            {
                var msg = "It does not contain a colon character.";

                throw new ArgumentException(msg, "header");
            }

            var name = header.Substring(0, idx);
            var val = idx < len - 1
                      ? header.Substring(idx + 1)
                      : String.Empty;

            name = checkName(name, "header");
            val = checkValue(val, "header");

            var headerType = getHeaderType(name);

            checkRestricted(name, headerType);
            checkAllowed(headerType);
            add(name, val, headerType);
        }

       /// <summary>
       /// 
       /// </summary>
       /// <param name="header"></param>
       /// <param name="value"></param>
        public void Add(HttpRequestHeader header, string value)
        {
            value = checkValue(value, "value");

            var key = header.ToString();
            var name = getHeaderName(key);

            checkRestricted(name, HttpHeaderType.Request);
            checkAllowed(HttpHeaderType.Request);
            add(name, value, HttpHeaderType.Request);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="header"></param>
        /// <param name="value"></param>
        public void Add(HttpResponseHeader header, string value)
        {
            value = checkValue(value, "value");

            var key = header.ToString();
            var name = getHeaderName(key);

            checkRestricted(name, HttpHeaderType.Response);
            checkAllowed(HttpHeaderType.Response);
            add(name, value, HttpHeaderType.Response);
        }

       /// <summary>
       /// 
       /// </summary>
       /// <param name="name"></param>
       /// <param name="value"></param>
        public override void Add(string name, string value)
        {
            name = checkName(name, "name");
            value = checkValue(value, "value");

            var headerType = getHeaderType(name);

            checkRestricted(name, headerType);
            checkAllowed(headerType);
            add(name, value, headerType);
        }

        /// <summary>
        /// Removes all headers from the collection.
        /// </summary>
        public override void Clear()
        {
            base.Clear();
            _state = HttpHeaderType.Unspecified;
        }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="index"></param>
      /// <returns></returns>
        public override string Get(int index)
        {
            return base.Get(index);
        }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="name"></param>
      /// <returns></returns>
        public override string Get(string name)
        {
            return base.Get(name);
        }

        /// <summary>
        /// Gets the enumerator used to iterate through the collection.
        /// </summary>
        /// <returns>
        /// An <see cref="IEnumerator"/> instance used to iterate through
        /// the collection.
        /// </returns>
        public override IEnumerator GetEnumerator()
        {
            return base.GetEnumerator();
        }

     /// <summary>
     /// 
     /// </summary>
     /// <param name="index"></param>
     /// <returns></returns>
        public override string GetKey(int index)
        {
            return base.GetKey(index);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public override string[] GetValues(int index)
        {
            var vals = base.GetValues(index);

            return vals != null && vals.Length > 0 ? vals : null;
        }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="name"></param>
      /// <returns></returns>
        public override string[] GetValues(string name)
        {
            var vals = base.GetValues(name);

            return vals != null && vals.Length > 0 ? vals : null;
        }

     /// <summary>
     /// 
     /// </summary>
     /// <param name="serializationInfo"></param>
     /// <param name="streamingContext"></param>
        [
          SecurityPermission(
            SecurityAction.LinkDemand,
            Flags = SecurityPermissionFlag.SerializationFormatter
          )
        ]
        public override void GetObjectData(
          SerializationInfo serializationInfo, StreamingContext streamingContext
        )
        {
            if (serializationInfo == null)
                throw new ArgumentNullException("serializationInfo");

            serializationInfo.AddValue("InternallyUsed", _internallyUsed);
            serializationInfo.AddValue("State", (int)_state);

            var cnt = Count;

            serializationInfo.AddValue("Count", cnt);

            for (var i = 0; i < cnt; i++)
            {
                serializationInfo.AddValue(i.ToString(), GetKey(i));
                serializationInfo.AddValue((cnt + i).ToString(), Get(i));
            }
        }

        /// <summary>
        /// 确定是否可以为请求设置指定的HTTP头
        /// </summary>
        /// <param name="headerName"></param>
        /// <returns></returns>
        public static bool IsRestricted(string headerName)
        {
            return IsRestricted(headerName, false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="headerName"></param>
        /// <param name="response"></param>
        /// <returns></returns>
        public static bool IsRestricted(string headerName, bool response)
        {
            headerName = checkName(headerName, "headerName");

            return isRestricted(headerName, response);
        }

        /// <summary>
        /// 实现 <see cref="ISerializable"/> 接口并在反序列化完成时引发反序列化事件
        /// </summary>
        /// <param name="sender"></param>
        public override void OnDeserialization(object sender)
        {
        }

        /// <summary>
        /// 从集合中移除指定的请求头
        /// </summary>
        /// <param name="header"></param>
        public void Remove(HttpRequestHeader header)
        {
            var key = header.ToString();
            var name = getHeaderName(key);

            checkRestricted(name, HttpHeaderType.Request);
            checkAllowed(HttpHeaderType.Request);
            base.Remove(name);
        }

        /// <summary>
        /// 从集合中移除指定的响应头
        /// </summary>
        /// <param name="header"></param>
        public void Remove(HttpResponseHeader header)
        {
            var key = header.ToString();
            var name = getHeaderName(key);

            checkRestricted(name, HttpHeaderType.Response);
            checkAllowed(HttpHeaderType.Response);
            base.Remove(name);
        }

        /// <summary>
        /// 从集合中删除指定的标头
        /// </summary>
        /// <param name="name"></param>
        public override void Remove(string name)
        {
            name = checkName(name, "name");

            var headerType = getHeaderType(name);

            checkRestricted(name, headerType);
            checkAllowed(headerType);
            base.Remove(name);
        }

        /// <summary>
        /// 将指定的请求头设置为指定的值
        /// </summary>
        /// <param name="header"></param>
        /// <param name="value"></param>
        public void Set(HttpRequestHeader header, string value)
        {
            value = checkValue(value, "value");

            var key = header.ToString();
            var name = getHeaderName(key);

            checkRestricted(name, HttpHeaderType.Request);
            checkAllowed(HttpHeaderType.Request);
            set(name, value, HttpHeaderType.Request);
        }

       /// <summary>
       /// 
       /// </summary>
       /// <param name="header"></param>
       /// <param name="value"></param>
        public void Set(HttpResponseHeader header, string value)
        {
            value = checkValue(value, "value");

            var key = header.ToString();
            var name = getHeaderName(key);

            checkRestricted(name, HttpHeaderType.Response);
            checkAllowed(HttpHeaderType.Response);
            set(name, value, HttpHeaderType.Response);
        }

       /// <summary>
       /// 
       /// </summary>
       /// <param name="name"></param>
       /// <param name="value"></param>
        public override void Set(string name, string value)
        {
            name = checkName(name, "name");
            value = checkValue(value, "value");

            var headerType = getHeaderType(name);

            checkRestricted(name, headerType);
            checkAllowed(headerType);
            set(name, value, headerType);
        }

        /// <summary>
        /// 将当前实例转换为字节数组
        /// </summary>
        /// <returns></returns>
        public byte[] ToByteArray()
        {
            return Encoding.UTF8.GetBytes(ToString());
        }

        /// <summary>
        /// 返回代表当前实例的字符串
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var cnt = Count;

            if (cnt == 0)
                return "\r\n";

            var buff = new StringBuilder();

            for (var i = 0; i < cnt; i++)
                buff.AppendFormat("{0}: {1}\r\n", GetKey(i), Get(i));

            buff.Append("\r\n");

            return buff.ToString();
        }

       /// <summary>
       /// 
       /// </summary>
       /// <param name="serializationInfo"></param>
       /// <param name="streamingContext"></param>
        [
          SecurityPermission(
            SecurityAction.LinkDemand,
            Flags = SecurityPermissionFlag.SerializationFormatter,
            SerializationFormatter = true
          )
        ]
        void ISerializable.GetObjectData(
          SerializationInfo serializationInfo, StreamingContext streamingContext
        )
        {
            GetObjectData(serializationInfo, streamingContext);
        }
    }
}
