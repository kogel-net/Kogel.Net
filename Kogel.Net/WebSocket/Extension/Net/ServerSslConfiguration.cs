using System;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace Kogel.Net.WebSocket.Extension.Net
{
    /// <summary>
    /// 存储服务器使用的 <see cref="SslStream"/> 的参数
    /// </summary>
    public class ServerSslConfiguration
    {
        private bool _checkCertRevocation;
        private bool _clientCertRequired;
        private RemoteCertificateValidationCallback _clientCertValidationCallback;
        private SslProtocols _enabledSslProtocols;
        private X509Certificate2 _serverCert;

        /// <summary>
        /// 
        /// </summary>
        public ServerSslConfiguration()
        {
            _enabledSslProtocols = SslProtocols.None;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="configuration"></param>
        public ServerSslConfiguration(ServerSslConfiguration configuration)
        {
            if (configuration == null)
                throw new ArgumentNullException("configuration");

            _checkCertRevocation = configuration._checkCertRevocation;
            _clientCertRequired = configuration._clientCertRequired;
            _clientCertValidationCallback = configuration._clientCertValidationCallback;
            _enabledSslProtocols = configuration._enabledSslProtocols;
            _serverCert = configuration._serverCert;
        }

        /// <summary>
        /// 
        /// </summary>
        public bool CheckCertificateRevocation
        {
            get
            {
                return _checkCertRevocation;
            }

            set
            {
                _checkCertRevocation = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool ClientCertificateRequired
        {
            get
            {
                return _clientCertRequired;
            }

            set
            {
                _clientCertRequired = value;
            }
        }

        /// <summary>
        /// 获取或设置用于验证客户端提供的证书的回调
        /// </summary>
        public RemoteCertificateValidationCallback ClientCertificateValidationCallback
        {
            get
            {
                if (_clientCertValidationCallback == null)
                    _clientCertValidationCallback = defaultValidateClientCertificate;

                return _clientCertValidationCallback;
            }

            set
            {
                _clientCertValidationCallback = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public SslProtocols EnabledSslProtocols
        {
            get
            {
                return _enabledSslProtocols;
            }

            set
            {
                _enabledSslProtocols = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public X509Certificate2 ServerCertificate
        {
            get
            {
                return _serverCert;
            }

            set
            {
                _serverCert = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="certificate"></param>
        /// <param name="chain"></param>
        /// <param name="sslPolicyErrors"></param>
        /// <returns></returns>
        private static bool defaultValidateClientCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }
    }
}
