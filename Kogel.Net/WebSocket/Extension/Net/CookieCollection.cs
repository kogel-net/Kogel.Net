using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Kogel.Net.WebSocket.Extension.Net
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class CookieCollection : ICollection<Cookie>
    {
        private List<Cookie> _list;
        private bool _readOnly;
        private object _sync;

        /// <summary>
        /// 
        /// </summary>
        public CookieCollection()
        {
            _list = new List<Cookie>();
            _sync = ((ICollection)_list).SyncRoot;
        }

        internal IList<Cookie> List
        {
            get
            {
                return _list;
            }
        }

        internal IEnumerable<Cookie> Sorted
        {
            get
            {
                var list = new List<Cookie>(_list);
                if (list.Count > 1)
                    list.Sort(compareForSorted);

                return list;
            }
        }





        /// <summary>
        /// 
        /// </summary>
        public int Count
        {
            get
            {
                return _list.Count;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool IsReadOnly
        {
            get
            {
                return _readOnly;
            }

            internal set
            {
                _readOnly = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool IsSynchronized
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Cookie this[int index]
        {
            get
            {
                if (index < 0 || index >= _list.Count)
                    throw new ArgumentOutOfRangeException("index");

                return _list[index];
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Cookie this[string name]
        {
            get
            {
                if (name == null)
                    throw new ArgumentNullException("name");

                var caseInsensitive = StringComparison.InvariantCultureIgnoreCase;

                foreach (var cookie in Sorted)
                {
                    if (cookie.Name.Equals(name, caseInsensitive))
                        return cookie;
                }

                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public object SyncRoot
        {
            get
            {
                return _sync;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cookie"></param>
        private void add(Cookie cookie)
        {
            var idx = search(cookie);
            if (idx == -1)
            {
                _list.Add(cookie);
                return;
            }

            _list[idx] = cookie;
        }

        private static int compareForSort(Cookie x, Cookie y)
        {
            return (x.Name.Length + x.Value.Length)
                   - (y.Name.Length + y.Value.Length);
        }

        private static int compareForSorted(Cookie x, Cookie y)
        {
            var ret = x.Version - y.Version;
            return ret != 0
                   ? ret
                   : (ret = x.Name.CompareTo(y.Name)) != 0
                     ? ret
                     : y.Path.Length - x.Path.Length;
        }

        private static CookieCollection parseRequest(string value)
        {
            var ret = new CookieCollection();

            Cookie cookie = null;
            var ver = 0;

            var caseInsensitive = StringComparison.InvariantCultureIgnoreCase;
            var pairs = value.SplitHeaderValue(',', ';').ToList();

            for (var i = 0; i < pairs.Count; i++)
            {
                var pair = pairs[i].Trim();
                if (pair.Length == 0)
                    continue;

                var idx = pair.IndexOf('=');
                if (idx == -1)
                {
                    if (cookie == null)
                        continue;

                    if (pair.Equals("$port", caseInsensitive))
                    {
                        cookie.Port = "\"\"";
                        continue;
                    }

                    continue;
                }

                if (idx == 0)
                {
                    if (cookie != null)
                    {
                        ret.add(cookie);
                        cookie = null;
                    }

                    continue;
                }

                var name = pair.Substring(0, idx).TrimEnd(' ');
                var val = idx < pair.Length - 1
                          ? pair.Substring(idx + 1).TrimStart(' ')
                          : String.Empty;

                if (name.Equals("$version", caseInsensitive))
                {
                    if (val.Length == 0)
                        continue;

                    int num;
                    if (!Int32.TryParse(val.Unquote(), out num))
                        continue;

                    ver = num;
                    continue;
                }

                if (name.Equals("$path", caseInsensitive))
                {
                    if (cookie == null)
                        continue;

                    if (val.Length == 0)
                        continue;

                    cookie.Path = val;
                    continue;
                }

                if (name.Equals("$domain", caseInsensitive))
                {
                    if (cookie == null)
                        continue;

                    if (val.Length == 0)
                        continue;

                    cookie.Domain = val;
                    continue;
                }

                if (name.Equals("$port", caseInsensitive))
                {
                    if (cookie == null)
                        continue;

                    if (val.Length == 0)
                        continue;

                    cookie.Port = val;
                    continue;
                }

                if (cookie != null)
                    ret.add(cookie);

                if (!Cookie.TryCreate(name, val, out cookie))
                    continue;

                if (ver != 0)
                    cookie.Version = ver;
            }

            if (cookie != null)
                ret.add(cookie);

            return ret;
        }

        private static CookieCollection parseResponse(string value)
        {
            var ret = new CookieCollection();

            Cookie cookie = null;

            var caseInsensitive = StringComparison.InvariantCultureIgnoreCase;
            var pairs = value.SplitHeaderValue(',', ';').ToList();

            for (var i = 0; i < pairs.Count; i++)
            {
                var pair = pairs[i].Trim();
                if (pair.Length == 0)
                    continue;

                var idx = pair.IndexOf('=');
                if (idx == -1)
                {
                    if (cookie == null)
                        continue;

                    if (pair.Equals("port", caseInsensitive))
                    {
                        cookie.Port = "\"\"";
                        continue;
                    }

                    if (pair.Equals("discard", caseInsensitive))
                    {
                        cookie.Discard = true;
                        continue;
                    }

                    if (pair.Equals("secure", caseInsensitive))
                    {
                        cookie.Secure = true;
                        continue;
                    }

                    if (pair.Equals("httponly", caseInsensitive))
                    {
                        cookie.HttpOnly = true;
                        continue;
                    }

                    continue;
                }

                if (idx == 0)
                {
                    if (cookie != null)
                    {
                        ret.add(cookie);
                        cookie = null;
                    }

                    continue;
                }

                var name = pair.Substring(0, idx).TrimEnd(' ');
                var val = idx < pair.Length - 1
                          ? pair.Substring(idx + 1).TrimStart(' ')
                          : String.Empty;

                if (name.Equals("version", caseInsensitive))
                {
                    if (cookie == null)
                        continue;

                    if (val.Length == 0)
                        continue;

                    int num;
                    if (!Int32.TryParse(val.Unquote(), out num))
                        continue;

                    cookie.Version = num;
                    continue;
                }

                if (name.Equals("expires", caseInsensitive))
                {
                    if (val.Length == 0)
                        continue;

                    if (i == pairs.Count - 1)
                        break;

                    i++;

                    if (cookie == null)
                        continue;

                    if (cookie.Expires != DateTime.MinValue)
                        continue;

                    var buff = new StringBuilder(val, 32);
                    buff.AppendFormat(", {0}", pairs[i].Trim());

                    DateTime expires;
                    if (
                      !DateTime.TryParseExact(
                        buff.ToString(),
                        new[] { "ddd, dd'-'MMM'-'yyyy HH':'mm':'ss 'GMT'", "r" },
                        CultureInfo.CreateSpecificCulture("en-US"),
                        DateTimeStyles.AdjustToUniversal
                        | DateTimeStyles.AssumeUniversal,
                        out expires
                      )
                    )
                        continue;

                    cookie.Expires = expires.ToLocalTime();
                    continue;
                }

                if (name.Equals("max-age", caseInsensitive))
                {
                    if (cookie == null)
                        continue;

                    if (val.Length == 0)
                        continue;

                    int num;
                    if (!Int32.TryParse(val.Unquote(), out num))
                        continue;

                    cookie.MaxAge = num;
                    continue;
                }

                if (name.Equals("path", caseInsensitive))
                {
                    if (cookie == null)
                        continue;

                    if (val.Length == 0)
                        continue;

                    cookie.Path = val;
                    continue;
                }

                if (name.Equals("domain", caseInsensitive))
                {
                    if (cookie == null)
                        continue;

                    if (val.Length == 0)
                        continue;

                    cookie.Domain = val;
                    continue;
                }

                if (name.Equals("port", caseInsensitive))
                {
                    if (cookie == null)
                        continue;

                    if (val.Length == 0)
                        continue;

                    cookie.Port = val;
                    continue;
                }

                if (name.Equals("comment", caseInsensitive))
                {
                    if (cookie == null)
                        continue;

                    if (val.Length == 0)
                        continue;

                    cookie.Comment = urlDecode(val, Encoding.UTF8);
                    continue;
                }

                if (name.Equals("commenturl", caseInsensitive))
                {
                    if (cookie == null)
                        continue;

                    if (val.Length == 0)
                        continue;

                    cookie.CommentUri = val.Unquote().ToUri();
                    continue;
                }

                if (name.Equals("samesite", caseInsensitive))
                {
                    if (cookie == null)
                        continue;

                    if (val.Length == 0)
                        continue;

                    cookie.SameSite = val.Unquote();
                    continue;
                }

                if (cookie != null)
                    ret.add(cookie);

                Cookie.TryCreate(name, val, out cookie);
            }

            if (cookie != null)
                ret.add(cookie);

            return ret;
        }

        private int search(Cookie cookie)
        {
            for (var i = _list.Count - 1; i >= 0; i--)
            {
                if (_list[i].EqualsWithoutValue(cookie))
                    return i;
            }

            return -1;
        }

        private static string urlDecode(string s, Encoding encoding)
        {
            if (s.IndexOfAny(new[] { '%', '+' }) == -1)
                return s;

            try
            {
                return HttpUtility.UrlDecode(s, encoding);
            }
            catch
            {
                return null;
            }
        }

        internal static CookieCollection Parse(string value, bool response)
        {
            try
            {
                return response
                       ? parseResponse(value)
                       : parseRequest(value);
            }
            catch (Exception ex)
            {
                throw new CookieException("It could not be parsed.", ex);
            }
        }

        internal void SetOrRemove(Cookie cookie)
        {
            var idx = search(cookie);
            if (idx == -1)
            {
                if (cookie.Expired)
                    return;

                _list.Add(cookie);
                return;
            }

            if (cookie.Expired)
            {
                _list.RemoveAt(idx);
                return;
            }

            _list[idx] = cookie;
        }

        internal void SetOrRemove(CookieCollection cookies)
        {
            foreach (var cookie in cookies._list)
                SetOrRemove(cookie);
        }

        internal void Sort()
        {
            if (_list.Count > 1)
                _list.Sort(compareForSort);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cookie"></param>
        public void Add(Cookie cookie)
        {
            if (_readOnly)
            {
                var msg = "The collection is read-only.";
                throw new InvalidOperationException(msg);
            }

            if (cookie == null)
                throw new ArgumentNullException("cookie");

            add(cookie);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cookies"></param>
        public void Add(CookieCollection cookies)
        {
            if (_readOnly)
            {
                var msg = "The collection is read-only.";
                throw new InvalidOperationException(msg);
            }

            if (cookies == null)
                throw new ArgumentNullException("cookies");

            foreach (var cookie in cookies._list)
                add(cookie);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Clear()
        {
            if (_readOnly)
            {
                var msg = "The collection is read-only.";
                throw new InvalidOperationException(msg);
            }

            _list.Clear();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cookie"></param>
        /// <returns></returns>
        public bool Contains(Cookie cookie)
        {
            if (cookie == null)
                throw new ArgumentNullException("cookie");

            return search(cookie) > -1;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="array"></param>
        /// <param name="index"></param>
        public void CopyTo(Cookie[] array, int index)
        {
            if (array == null)
                throw new ArgumentNullException("array");

            if (index < 0)
                throw new ArgumentOutOfRangeException("index", "Less than zero.");

            if (array.Length - index < _list.Count)
            {
                var msg = "The available space of the array is not enough to copy to.";
                throw new ArgumentException(msg);
            }

            _list.CopyTo(array, index);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerator<Cookie> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cookie"></param>
        /// <returns></returns>
        public bool Remove(Cookie cookie)
        {
            if (_readOnly)
            {
                var msg = "The collection is read-only.";
                throw new InvalidOperationException(msg);
            }

            if (cookie == null)
                throw new ArgumentNullException("cookie");

            var idx = search(cookie);
            if (idx == -1)
                return false;

            _list.RemoveAt(idx);
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return _list.GetEnumerator();
        }
    }
}
