using System;
using System.Security.Principal;

namespace Kogel.Net.WebSocket.Extension.Net
{
    /// <summary>
    /// 保存来自 HTTP 基本身份验证尝试的用户名和密码
    /// </summary>
    public class HttpBasicIdentity : GenericIdentity
    {
        private string _password;
        internal HttpBasicIdentity(string username, string password) : base(username, "Basic")
        {
            _password = password;
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual string Password
        {
            get
            {
                return _password;
            }
        }
    }
}
