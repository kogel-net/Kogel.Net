using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kogel.Net.WebSocket.Extension.Net
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public sealed class Cookie
    {
        private string _comment;
        private Uri _commentUri;
        private bool _discard;
        private string _domain;
        private static readonly int[] _emptyPorts;
        private DateTime _expires;
        private bool _httpOnly;
        private string _name;
        private string _path;
        private string _port;
        private int[] _ports;
        private static readonly char[] _reservedCharsForValue;
        private string _sameSite;
        private bool _secure;
        private DateTime _timeStamp;
        private string _value;
        private int _version;

        static Cookie()
        {
            _emptyPorts = new int[0];
            _reservedCharsForValue = new[] { ';', ',' };
        }

        /// <summary>
        /// 初始化 <see cref="Cookie"/> 类的新实例
        /// </summary>
        internal Cookie()
        {
            init(String.Empty, String.Empty, String.Empty, String.Empty);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public Cookie(string name, string value)
          : this(name, value, String.Empty, String.Empty)
        {
        }

        /// <summary>
        /// 使用指定的名称、值和路径初始化 <see cref="Cookie"/> 类的新实例
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="path"></param>
        public Cookie(string name, string value, string path)
          : this(name, value, path, String.Empty)
        {
        }

        /// <summary>
        /// 使用指定的名称、值、路径和域初始化 <see cref="Cookie"/> 类的新实例
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="path"></param>
        /// <param name="domain"></param>
        public Cookie(string name, string value, string path, string domain)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            if (name.Length == 0)
                throw new ArgumentException("An empty string.", "name");

            if (name[0] == '$')
            {
                var msg = "It starts with a dollar sign.";
                throw new ArgumentException(msg, "name");
            }

            if (!name.IsToken())
            {
                var msg = "It contains an invalid character.";
                throw new ArgumentException(msg, "name");
            }

            if (value == null)
                value = String.Empty;

            if (value.Contains(_reservedCharsForValue))
            {
                if (!value.IsEnclosedIn('"'))
                {
                    var msg = "A string not enclosed in double quotes.";
                    throw new ArgumentException(msg, "value");
                }
            }

            init(name, value, path ?? String.Empty, domain ?? String.Empty);
        }

        /// <summary>
        /// 
        /// </summary>
        internal bool ExactDomain
        {
            get
            {
                return _domain.Length == 0 || _domain[0] != '.';
            }
        }

        /// <summary>
        /// 
        /// </summary>
        internal int MaxAge
        {
            get
            {
                if (_expires == DateTime.MinValue)
                    return 0;

                var expires = _expires.Kind != DateTimeKind.Local
                              ? _expires.ToLocalTime()
                              : _expires;

                var span = expires - DateTime.Now;
                return span > TimeSpan.Zero
                       ? (int)span.TotalSeconds
                       : 0;
            }

            set
            {
                _expires = value > 0
                           ? DateTime.Now.AddSeconds((double)value)
                           : DateTime.Now;
            }
        }

        internal int[] Ports
        {
            get
            {
                return _ports ?? _emptyPorts;
            }
        }

        internal string SameSite
        {
            get
            {
                return _sameSite;
            }

            set
            {
                _sameSite = value;
            }
        }

        /// <summary>
        /// 获取cookie的Comment属性值
        /// </summary>
        public string Comment
        {
            get
            {
                return _comment;
            }

            internal set
            {
                _comment = value;
            }
        }

        /// <summary>
        /// 获取 cookie 的 CommentURL 属性值
        /// </summary>
        public Uri CommentUri
        {
            get
            {
                return _commentUri;
            }

            internal set
            {
                _commentUri = value;
            }
        }

        /// <summary>
        /// 获取一个值，该值指示客户端是否在客户端终止时无条件丢弃 cookie
        /// </summary>
        public bool Discard
        {
            get
            {
                return _discard;
            }

            internal set
            {
                _discard = value;
            }
        }

        /// <summary>
        /// 获取或设置cookie的Domain属性值
        /// </summary>
        public string Domain
        {
            get
            {
                return _domain;
            }

            set
            {
                _domain = value ?? String.Empty;
            }
        }

        /// <summary>
        /// 获取或设置一个值，该值指示 cookie 是否已过期
        /// </summary>
        public bool Expired
        {
            get
            {
                return _expires != DateTime.MinValue && _expires <= DateTime.Now;
            }

            set
            {
                _expires = value ? DateTime.Now : DateTime.MinValue;
            }
        }

        /// <summary>
        /// 获取或设置cookie的Expires属性值
        /// </summary>
        public DateTime Expires
        {
            get
            {
                return _expires;
            }

            set
            {
                _expires = value;
            }
        }

        /// <summary>
        /// 获取或设置一个值，该值指示非 HTTP API 是否可以访问 cookie
        /// </summary>
        public bool HttpOnly
        {
            get
            {
                return _httpOnly;
            }

            set
            {
                _httpOnly = value;
            }
        }

        /// <summary>
        /// 获取或设置cookie的名称
        /// </summary>
        public string Name
        {
            get
            {
                return _name;
            }

            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");

                if (value.Length == 0)
                    throw new ArgumentException("An empty string.", "value");

                if (value[0] == '$')
                {
                    var msg = "It starts with a dollar sign.";
                    throw new ArgumentException(msg, "value");
                }

                if (!value.IsToken())
                {
                    var msg = "It contains an invalid character.";
                    throw new ArgumentException(msg, "value");
                }

                _name = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string Path
        {
            get
            {
                return _path;
            }

            set
            {
                _path = value ?? String.Empty;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string Port
        {
            get
            {
                return _port;
            }

            internal set
            {
                int[] ports;
                if (!tryCreatePorts(value, out ports))
                    return;

                _port = value;
                _ports = ports;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool Secure
        {
            get
            {
                return _secure;
            }

            set
            {
                _secure = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public DateTime TimeStamp
        {
            get
            {
                return _timeStamp;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string Value
        {
            get
            {
                return _value;
            }

            set
            {
                if (value == null)
                    value = String.Empty;

                if (value.Contains(_reservedCharsForValue))
                {
                    if (!value.IsEnclosedIn('"'))
                    {
                        var msg = "A string not enclosed in double quotes.";
                        throw new ArgumentException(msg, "value");
                    }
                }

                _value = value;
            }
        }

       /// <summary>
       /// 
       /// </summary>
        public int Version
        {
            get
            {
                return _version;
            }

            internal set
            {
                if (value < 0 || value > 1)
                    return;

                _version = value;
            }
        }

        private static int hash(int i, int j, int k, int l, int m)
        {
            return i
                   ^ (j << 13 | j >> 19)
                   ^ (k << 26 | k >> 6)
                   ^ (l << 7 | l >> 25)
                   ^ (m << 20 | m >> 12);
        }

        private void init(string name, string value, string path, string domain)
        {
            _name = name;
            _value = value;
            _path = path;
            _domain = domain;

            _expires = DateTime.MinValue;
            _timeStamp = DateTime.Now;
        }

        private string toResponseStringVersion0()
        {
            var buff = new StringBuilder(64);

            buff.AppendFormat("{0}={1}", _name, _value);

            if (_expires != DateTime.MinValue)
            {
                buff.AppendFormat(
                  "; Expires={0}",
                  _expires.ToUniversalTime().ToString(
                    "ddd, dd'-'MMM'-'yyyy HH':'mm':'ss 'GMT'",
                    CultureInfo.CreateSpecificCulture("en-US")
                  )
                );
            }

            if (!_path.IsNullOrEmpty())
                buff.AppendFormat("; Path={0}", _path);

            if (!_domain.IsNullOrEmpty())
                buff.AppendFormat("; Domain={0}", _domain);

            if (!_sameSite.IsNullOrEmpty())
                buff.AppendFormat("; SameSite={0}", _sameSite);

            if (_secure)
                buff.Append("; Secure");

            if (_httpOnly)
                buff.Append("; HttpOnly");

            return buff.ToString();
        }

        private string toResponseStringVersion1()
        {
            var buff = new StringBuilder(64);

            buff.AppendFormat("{0}={1}; Version={2}", _name, _value, _version);

            if (_expires != DateTime.MinValue)
                buff.AppendFormat("; Max-Age={0}", MaxAge);

            if (!_path.IsNullOrEmpty())
                buff.AppendFormat("; Path={0}", _path);

            if (!_domain.IsNullOrEmpty())
                buff.AppendFormat("; Domain={0}", _domain);

            if (_port != null)
            {
                if (_port != "\"\"")
                    buff.AppendFormat("; Port={0}", _port);
                else
                    buff.Append("; Port");
            }

            if (_comment != null)
                buff.AppendFormat("; Comment={0}", HttpUtility.UrlEncode(_comment));

            if (_commentUri != null)
            {
                var url = _commentUri.OriginalString;
                buff.AppendFormat(
                  "; CommentURL={0}", !url.IsToken() ? url.Quote() : url
                );
            }

            if (_discard)
                buff.Append("; Discard");

            if (_secure)
                buff.Append("; Secure");

            return buff.ToString();
        }

        private static bool tryCreatePorts(string value, out int[] result)
        {
            result = null;

            var arr = value.Trim('"').Split(',');
            var len = arr.Length;
            var res = new int[len];

            for (var i = 0; i < len; i++)
            {
                var s = arr[i].Trim();
                if (s.Length == 0)
                {
                    res[i] = Int32.MinValue;
                    continue;
                }

                if (!Int32.TryParse(s, out res[i]))
                    return false;
            }

            result = res;
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cookie"></param>
        /// <returns></returns>
        internal bool EqualsWithoutValue(Cookie cookie)
        {
            var caseSensitive = StringComparison.InvariantCulture;
            var caseInsensitive = StringComparison.InvariantCultureIgnoreCase;

            return _name.Equals(cookie._name, caseInsensitive)
                   && _path.Equals(cookie._path, caseSensitive)
                   && _domain.Equals(cookie._domain, caseInsensitive)
                   && _version == cookie._version;
        }

        internal bool EqualsWithoutValueAndVersion(Cookie cookie)
        {
            var caseSensitive = StringComparison.InvariantCulture;
            var caseInsensitive = StringComparison.InvariantCultureIgnoreCase;

            return _name.Equals(cookie._name, caseInsensitive)
                   && _path.Equals(cookie._path, caseSensitive)
                   && _domain.Equals(cookie._domain, caseInsensitive);
        }

        internal string ToRequestString(Uri uri)
        {
            if (_name.Length == 0)
                return String.Empty;

            if (_version == 0)
                return String.Format("{0}={1}", _name, _value);

            var buff = new StringBuilder(64);

            buff.AppendFormat("$Version={0}; {1}={2}", _version, _name, _value);

            if (!_path.IsNullOrEmpty())
                buff.AppendFormat("; $Path={0}", _path);
            else if (uri != null)
                buff.AppendFormat("; $Path={0}", uri.GetAbsolutePath());
            else
                buff.Append("; $Path=/");

            if (!_domain.IsNullOrEmpty())
            {
                if (uri == null || uri.Host != _domain)
                    buff.AppendFormat("; $Domain={0}", _domain);
            }

            if (_port != null)
            {
                if (_port != "\"\"")
                    buff.AppendFormat("; $Port={0}", _port);
                else
                    buff.Append("; $Port");
            }

            return buff.ToString();
        }

        /// <summary>
        /// 返回表示当前 cookie 实例的字符串
        /// </summary>
        /// <returns></returns>
        internal string ToResponseString()
        {
            return _name.Length == 0
                   ? String.Empty
                   : _version == 0
                     ? toResponseStringVersion0()
                     : toResponseStringVersion1();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        internal static bool TryCreate(string name, string value, out Cookie result)
        {
            result = null;

            try
            {
                result = new Cookie(name, value);
            }
            catch
            {
                return false;
            }

            return true;
        }


        /// <summary>
        /// 判断当前cookie实例是否等于指定的<see cref="object"/>实例
        /// </summary>
        /// <param name="comparand"></param>
        /// <returns></returns>
        public override bool Equals(object comparand)
        {
            var cookie = comparand as Cookie;
            if (cookie == null)
                return false;

            var caseSensitive = StringComparison.InvariantCulture;
            var caseInsensitive = StringComparison.InvariantCultureIgnoreCase;

            return _name.Equals(cookie._name, caseInsensitive)
                   && _value.Equals(cookie._value, caseSensitive)
                   && _path.Equals(cookie._path, caseSensitive)
                   && _domain.Equals(cookie._domain, caseInsensitive)
                   && _version == cookie._version;
        }

        /// <summary>
        /// 获取当前 cookie 实例的hash
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return hash(
                     StringComparer.InvariantCultureIgnoreCase.GetHashCode(_name),
                     _value.GetHashCode(),
                     _path.GetHashCode(),
                     StringComparer.InvariantCultureIgnoreCase.GetHashCode(_domain),
                     _version
                   );
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return ToRequestString(null);
        }
    }
}
