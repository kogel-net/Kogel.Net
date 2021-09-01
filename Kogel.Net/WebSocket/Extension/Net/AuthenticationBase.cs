using Kogel.Net.WebSocket.Enums;
using System;
using System.Collections.Specialized;
using System.Text;

namespace Kogel.Net.WebSocket.Extension.Net
{
    /// <summary>
    /// 
    /// </summary>
    internal abstract class AuthenticationBase
    {
        /// <summary>
        /// 
        /// </summary>
        private AuthenticationSchemes _scheme;

        /// <summary>
        /// 
        /// </summary>
        internal NameValueCollection Parameters;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="scheme"></param>
        /// <param name="parameters"></param>
        protected AuthenticationBase(AuthenticationSchemes scheme, NameValueCollection parameters)
        {
            _scheme = scheme;
            Parameters = parameters;
        }

        public string Algorithm
        {
            get
            {
                return Parameters["algorithm"];
            }
        }

        public string Nonce
        {
            get
            {
                return Parameters["nonce"];
            }
        }

        public string Opaque
        {
            get
            {
                return Parameters["opaque"];
            }
        }

        public string Qop
        {
            get
            {
                return Parameters["qop"];
            }
        }

        public string Realm
        {
            get
            {
                return Parameters["realm"];
            }
        }

        public AuthenticationSchemes Scheme
        {
            get
            {
                return _scheme;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        internal static string CreateNonceValue()
        {
            var src = new byte[16];
            var rand = new Random();
            rand.NextBytes(src);

            var res = new StringBuilder(32);
            foreach (var b in src)
                res.Append(b.ToString("x2"));

            return res.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        internal static NameValueCollection ParseParameters(string value)
        {
            var res = new NameValueCollection();
            foreach (var param in value.SplitHeaderValue(','))
            {
                var i = param.IndexOf('=');
                var name = i > 0 ? param.Substring(0, i).Trim() : null;
                var val = i < 0
                          ? param.Trim().Trim('"')
                          : i < param.Length - 1
                            ? param.Substring(i + 1).Trim().Trim('"')
                            : String.Empty;

                res.Add(name, val);
            }

            return res;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        internal abstract string ToBasicString();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        internal abstract string ToDigestString();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return _scheme == AuthenticationSchemes.Basic
                   ? ToBasicString()
                   : _scheme == AuthenticationSchemes.Digest
                     ? ToDigestString()
                     : String.Empty;
        }
    }
}
