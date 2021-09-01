using System;
using System.Collections.Specialized;
using System.Text;

namespace Kogel.Net.WebSocket.Extension.Net
{
    /// <summary>
    /// 
    /// </summary>
    internal sealed class QueryStringCollection : NameValueCollection
    {

        public QueryStringCollection()
        {
        }

        public QueryStringCollection(int capacity) : base(capacity)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        private static string urlDecode(string s, Encoding encoding)
        {
            return s.IndexOfAny(new[] { '%', '+' }) > -1
                   ? HttpUtility.UrlDecode(s, encoding)
                   : s;
        }

        public static QueryStringCollection Parse(string query)
        {
            return Parse(query, Encoding.UTF8);
        }

        public static QueryStringCollection Parse(string query, Encoding encoding)
        {
            if (query == null)
                return new QueryStringCollection(1);

            var len = query.Length;
            if (len == 0)
                return new QueryStringCollection(1);

            if (query == "?")
                return new QueryStringCollection(1);

            if (query[0] == '?')
                query = query.Substring(1);

            if (encoding == null)
                encoding = Encoding.UTF8;

            var ret = new QueryStringCollection();

            var components = query.Split('&');
            foreach (var component in components)
            {
                len = component.Length;
                if (len == 0)
                    continue;

                if (component == "=")
                    continue;

                var i = component.IndexOf('=');
                if (i < 0)
                {
                    ret.Add(null, urlDecode(component, encoding));
                    continue;
                }

                if (i == 0)
                {
                    ret.Add(null, urlDecode(component.Substring(1), encoding));
                    continue;
                }

                var name = urlDecode(component.Substring(0, i), encoding);

                var start = i + 1;
                var val = start < len
                          ? urlDecode(component.Substring(start), encoding)
                          : String.Empty;

                ret.Add(name, val);
            }

            return ret;
        }

        public override string ToString()
        {
            var buff = new StringBuilder();

            foreach (var key in AllKeys)
                buff.AppendFormat("{0}={1}&", key, this[key]);

            if (buff.Length > 0)
                buff.Length--;

            return buff.ToString();
        }
    }
}
