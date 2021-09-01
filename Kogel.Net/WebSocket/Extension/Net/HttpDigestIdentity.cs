using System;
using System.Collections.Specialized;
using System.Security.Principal;

namespace Kogel.Net.WebSocket.Extension.Net
{
    /// <summary>
    /// 保存来自 HTTP 摘要身份验证尝试的用户名和其他参数
    /// </summary>
    public class HttpDigestIdentity : GenericIdentity
    {
        private NameValueCollection _parameters;
        internal HttpDigestIdentity(NameValueCollection parameters)
          : base(parameters["username"], "Digest")
        {
            _parameters = parameters;
        }

        /// <summary>
        /// 从摘要认证尝试中获取算法参数
        /// </summary>
        public string Algorithm
        {
            get
            {
                return _parameters["algorithm"];
            }
        }

        /// <summary>
        /// 从摘要式身份验证尝试中获取 cnonce 参数
        /// </summary>
        public string Cnonce
        {
            get
            {
                return _parameters["cnonce"];
            }
        }

        /// <summary>
        /// 从摘要身份验证尝试中获取 nc 参数
        /// </summary>
        public string Nc
        {
            get
            {
                return _parameters["nc"];
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string Nonce
        {
            get
            {
                return _parameters["nonce"];
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string Opaque
        {
            get
            {
                return _parameters["opaque"];
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string Qop
        {
            get
            {
                return _parameters["qop"];
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string Realm
        {
            get
            {
                return _parameters["realm"];
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string Response
        {
            get
            {
                return _parameters["response"];
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string Uri
        {
            get
            {
                return _parameters["uri"];
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="password"></param>
        /// <param name="realm"></param>
        /// <param name="method"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        internal bool IsValid(string password, string realm, string method, string entity)
        {
            var copied = new NameValueCollection(_parameters);
            copied["password"] = password;
            copied["realm"] = realm;
            copied["method"] = method;
            copied["entity"] = entity;

            var expected = AuthenticationResponse.CreateRequestDigest(copied);
            return _parameters["response"] == expected;
        }
    }
}
