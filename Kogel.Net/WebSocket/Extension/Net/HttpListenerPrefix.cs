using System;

namespace Kogel.Net.WebSocket.Extension.Net
{
    /// <summary>
    /// HTTP侦听器前缀
    /// </summary>
    internal sealed class HttpListenerPrefix
    {
        private string _host;
        private HttpListener _listener;
        private string _original;
        private string _path;
        private string _port;
        private string _prefix;
        private bool _secure;


        /// <summary>
        /// 使用指定的 URI 前缀和 HTTP 侦听器初始化 <see cref="HttpListenerPrefix"/> 类的新实例。
        /// </summary>
        /// <param name="uriPrefix"></param>
        /// <param name="listener"></param>
        internal HttpListenerPrefix(string uriPrefix, HttpListener listener)
        {
            _original = uriPrefix;
            _listener = listener;

            parse(uriPrefix);
        }

        public string Host
        {
            get
            {
                return _host;
            }
        }

        public bool IsSecure
        {
            get
            {
                return _secure;
            }
        }

        public HttpListener Listener
        {
            get
            {
                return _listener;
            }
        }

        public string Original
        {
            get
            {
                return _original;
            }
        }

        public string Path
        {
            get
            {
                return _path;
            }
        }

        public string Port
        {
            get
            {
                return _port;
            }
        }

        private void parse(string uriPrefix)
        {
            if (uriPrefix.StartsWith("https"))
                _secure = true;

            var len = uriPrefix.Length;
            var host = uriPrefix.IndexOf(':') + 3;
            var root = uriPrefix.IndexOf('/', host + 1, len - host - 1);

            var colon = uriPrefix.LastIndexOf(':', root - 1, root - host - 1);

            if (uriPrefix[root - 1] != ']' && colon > host)
            {
                _host = uriPrefix.Substring(host, colon - host);
                _port = uriPrefix.Substring(colon + 1, root - colon - 1);
            }
            else
            {
                _host = uriPrefix.Substring(host, root - host);
                _port = _secure ? "443" : "80";
            }

            _path = uriPrefix.Substring(root);

            _prefix = String.Format(
                        "{0}://{1}:{2}{3}",
                        _secure ? "https" : "http",
                        _host,
                        _port,
                        _path
                      );
        }

        public static void CheckPrefix(string uriPrefix)
        {
            if (uriPrefix == null)
                throw new ArgumentNullException("uriPrefix");

            var len = uriPrefix.Length;

            if (len == 0)
            {
                var msg = "An empty string.";

                throw new ArgumentException(msg, "uriPrefix");
            }

            var schm = uriPrefix.StartsWith("http://")
                       || uriPrefix.StartsWith("https://");

            if (!schm)
            {
                var msg = "The scheme is not 'http' or 'https'.";

                throw new ArgumentException(msg, "uriPrefix");
            }

            var end = len - 1;

            if (uriPrefix[end] != '/')
            {
                var msg = "It ends without '/'.";

                throw new ArgumentException(msg, "uriPrefix");
            }

            var host = uriPrefix.IndexOf(':') + 3;

            if (host >= end)
            {
                var msg = "No host is specified.";

                throw new ArgumentException(msg, "uriPrefix");
            }

            if (uriPrefix[host] == ':')
            {
                var msg = "No host is specified.";

                throw new ArgumentException(msg, "uriPrefix");
            }

            var root = uriPrefix.IndexOf('/', host, len - host);

            if (root == host)
            {
                var msg = "No host is specified.";

                throw new ArgumentException(msg, "uriPrefix");
            }

            if (uriPrefix[root - 1] == ':')
            {
                var msg = "No port is specified.";

                throw new ArgumentException(msg, "uriPrefix");
            }

            if (root == end - 1)
            {
                var msg = "No path is specified.";

                throw new ArgumentException(msg, "uriPrefix");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            var pref = obj as HttpListenerPrefix;

            return pref != null && _prefix.Equals(pref._prefix);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return _prefix.GetHashCode();
        }

        public override string ToString()
        {
            return _prefix;
        }
    }
}
