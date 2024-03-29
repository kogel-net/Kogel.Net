﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kogel.Net.WebSocket.Extension.Net
{
    /// <summary>
    /// 
    /// </summary>
    internal class HttpResponse : HttpBase
    {
        private string _code;
        private string _reason;
        private HttpResponse(string code, string reason, Version version, NameValueCollection headers)
          : base(version, headers)
        {
            _code = code;
            _reason = reason;
        }

        internal HttpResponse(HttpStatusCode code) : this(code, code.GetDescription())
        {
        }

        internal HttpResponse(HttpStatusCode code, string reason) : this(((int)code).ToString(), reason, HttpVersion.Version11, new NameValueCollection())
        {
            Headers["Server"] = "websocket-sharp/1.0";
        }

        public CookieCollection Cookies
        {
            get
            {
                return Headers.GetCookies(true);
            }
        }

        public bool HasConnectionClose
        {
            get
            {
                var comparison = StringComparison.OrdinalIgnoreCase;
                return Headers.Contains("Connection", "close", comparison);
            }
        }

        public bool IsProxyAuthenticationRequired
        {
            get
            {
                return _code == "407";
            }
        }

        public bool IsRedirect
        {
            get
            {
                return _code == "301" || _code == "302";
            }
        }

        public bool IsUnauthorized
        {
            get
            {
                return _code == "401";
            }
        }

        public bool IsWebSocketResponse
        {
            get
            {
                return ProtocolVersion > HttpVersion.Version10
                       && _code == "101"
                       && Headers.Upgrades("websocket");
            }
        }

        public string Reason
        {
            get
            {
                return _reason;
            }
        }

        public string StatusCode
        {
            get
            {
                return _code;
            }
        }

        internal static HttpResponse CreateCloseResponse(HttpStatusCode code)
        {
            var res = new HttpResponse(code);
            res.Headers["Connection"] = "close";

            return res;
        }

        internal static HttpResponse CreateUnauthorizedResponse(string challenge)
        {
            var res = new HttpResponse(HttpStatusCode.Unauthorized);
            res.Headers["WWW-Authenticate"] = challenge;

            return res;
        }

        internal static HttpResponse CreateWebSocketResponse()
        {
            var res = new HttpResponse(HttpStatusCode.SwitchingProtocols);

            var headers = res.Headers;
            headers["Upgrade"] = "websocket";
            headers["Connection"] = "Upgrade";

            return res;
        }

        internal static HttpResponse Parse(string[] headerParts)
        {
            var statusLine = headerParts[0].Split(new[] { ' ' }, 3);
            if (statusLine.Length != 3)
                throw new ArgumentException("Invalid status line: " + headerParts[0]);

            var headers = new WebHeaderCollection();
            for (int i = 1; i < headerParts.Length; i++)
                headers.InternalSet(headerParts[i], true);

            return new HttpResponse(
              statusLine[1], statusLine[2], new Version(statusLine[0].Substring(5)), headers);
        }

        internal static HttpResponse Read(Stream stream, int millisecondsTimeout)
        {
            return Read<HttpResponse>(stream, Parse, millisecondsTimeout);
        }
        public void SetCookies(CookieCollection cookies)
        {
            if (cookies == null || cookies.Count == 0)
                return;

            var headers = Headers;
            foreach (var cookie in cookies.Sorted)
                headers.Add("Set-Cookie", cookie.ToResponseString());
        }

        public override string ToString()
        {
            var output = new StringBuilder(64);
            output.AppendFormat("HTTP/{0} {1} {2}{3}", ProtocolVersion, _code, _reason, CrLf);

            var headers = Headers;
            foreach (var key in headers.AllKeys)
                output.AppendFormat("{0}: {1}{2}", key, headers[key], CrLf);

            output.Append(CrLf);

            var entity = EntityBody;
            if (entity.Length > 0)
                output.Append(entity);

            return output.ToString();
        }
    }
}
