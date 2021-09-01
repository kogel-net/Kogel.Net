using System;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace Kogel.Net.WebSocket.Extension.Net
{
    /// <summary>
    /// 存储客户端使用的 <see cref="SslStream"/> 的参数
    /// </summary>
    public class ClientSslConfiguration
    {
        private bool _checkCertRevocation;
        private LocalCertificateSelectionCallback _clientCertSelectionCallback;
        private X509CertificateCollection _clientCerts;
        private SslProtocols _enabledSslProtocols;
        private RemoteCertificateValidationCallback _serverCertValidationCallback;
        private string _targetHost;

        /// <summary>
        /// 使用指定的目标主机服务器名称初始化 <see cref="ClientSslConfiguration"/> 类的新实例
        /// </summary>
        /// <param name="targetHost"></param>
        public ClientSslConfiguration(string targetHost)
        {
            if (targetHost == null)
                throw new ArgumentNullException("targetHost");

            if (targetHost.Length == 0)
                throw new ArgumentException("An empty string.", "targetHost");

            _targetHost = targetHost;

            _enabledSslProtocols = SslProtocols.None;
        }

        /// <summary>
        /// 初始化 <see cref="ClientSslConfiguration"/> 类的新实例，该类存储从指定配置复制的参数
        /// </summary>
        /// <param name="configuration"></param>
        public ClientSslConfiguration(ClientSslConfiguration configuration)
        {
            if (configuration == null)
                throw new ArgumentNullException("configuration");

            _checkCertRevocation = configuration._checkCertRevocation;
            _clientCertSelectionCallback = configuration._clientCertSelectionCallback;
            _clientCerts = configuration._clientCerts;
            _enabledSslProtocols = configuration._enabledSslProtocols;
            _serverCertValidationCallback = configuration._serverCertValidationCallback;
            _targetHost = configuration._targetHost;
        }

        /// <summary>
        /// 获取或设置一个值，该值指示在身份验证期间是否检查证书吊销列表
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
        /// 获取或设置客户端证书的集合，从中选择一个提供给服务器
        /// </summary>
        public X509CertificateCollection ClientCertificates
        {
            get
            {
                return _clientCerts;
            }

            set
            {
                _clientCerts = value;
            }
        }

        /// <summary>
        /// 获取或设置用于选择要提供给服务器的证书的回调
        /// </summary>
        public LocalCertificateSelectionCallback ClientCertificateSelectionCallback
        {
            get
            {
                if (_clientCertSelectionCallback == null)
                    _clientCertSelectionCallback = defaultSelectClientCertificate;

                return _clientCertSelectionCallback;
            }

            set
            {
                _clientCertSelectionCallback = value;
            }
        }

        /// <summary>
        /// 获取或设置用于身份验证的协议
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
        /// 获取或设置用于验证服务器提供的证书的回调
        /// </summary>
        public RemoteCertificateValidationCallback ServerCertificateValidationCallback
        {
            get
            {
                if (_serverCertValidationCallback == null)
                    _serverCertValidationCallback = defaultValidateServerCertificate;

                return _serverCertValidationCallback;
            }

            set
            {
                _serverCertValidationCallback = value;
            }
        }

        /// <summary>
        /// 获取或设置目标主机服务器名称
        /// </summary>
        public string TargetHost
        {
            get
            {
                return _targetHost;
            }

            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");

                if (value.Length == 0)
                    throw new ArgumentException("An empty string.", "value");

                _targetHost = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="targetHost"></param>
        /// <param name="clientCertificates"></param>
        /// <param name="serverCertificate"></param>
        /// <param name="acceptableIssuers"></param>
        /// <returns></returns>
        private static X509Certificate defaultSelectClientCertificate(object sender, string targetHost, X509CertificateCollection clientCertificates
            , X509Certificate serverCertificate
            , string[] acceptableIssuers
        )
        {
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="certificate"></param>
        /// <param name="chain"></param>
        /// <param name="sslPolicyErrors"></param>
        /// <returns></returns>
        private static bool defaultValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }
    }
}
