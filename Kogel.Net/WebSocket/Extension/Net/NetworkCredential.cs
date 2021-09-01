using System;

namespace Kogel.Net.WebSocket.Extension.Net
{
    /// <summary>
    /// 为基于密码的身份验证提供凭据
    /// </summary>
    public class NetworkCredential
    {
        private string _domain;
        private static readonly string[] _noRoles;
        private string _password;
        private string[] _roles;
        private string _username;

        static NetworkCredential()
        {
            _noRoles = new string[0];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        public NetworkCredential(string username, string password) : this(username, password, null, null)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="domain"></param>
        /// <param name="roles"></param>
        public NetworkCredential(string username, string password, string domain, params string[] roles)
        {
            if (username == null)
                throw new ArgumentNullException("username");

            if (username.Length == 0)
                throw new ArgumentException("An empty string.", "username");

            _username = username;
            _password = password;
            _domain = domain;
            _roles = roles;
        }

        /// <summary>
        /// 
        /// </summary>
        public string Domain
        {
            get
            {
                return _domain ?? String.Empty;
            }

            internal set
            {
                _domain = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string Password
        {
            get
            {
                return _password ?? String.Empty;
            }

            internal set
            {
                _password = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string[] Roles
        {
            get
            {
                return _roles ?? _noRoles;
            }

            internal set
            {
                _roles = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string Username
        {
            get
            {
                return _username;
            }

            internal set
            {
                _username = value;
            }
        }
    }
}
