using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Kogel.Http.Extension
{
    /// <summary>
    /// 请求
    /// </summary>
    public class KogelRequest
    {
        /// <summary>
        /// 请求的url
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// 请求类型(默认get)
        /// </summary>
        public string Method { get; set; } = "GET";

        /// <summary>
        /// 请求超时时间(默认90秒)
        /// </summary>
        public int Timeout { get; set; } = 90000;

        /// <summary>
        /// 写入post数据超时时间(默认30秒)
        /// </summary>
        public int WriteTimeout { get; set; } = 30000;

        /// <summary>
        /// 
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Boolean KeepAlive { get; set; }

        /// <summary>
        /// 请求标头(默认text/html, application/json, *)
        /// </summary>
        public string Accept { get; set; } = "text/html, application/json, */*";

        /// <summary>
        /// 请求返回类型(默认 application/json)
        /// </summary>
        public string ContentType { get; set; } = "application/json";

        /// <summary>
        /// 客户端访问信息默认平台版本
        /// </summary>
        public string UserAgent { get; set; } = "Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; Trident/5.0; kogel)";

        /// <summary>
        /// 数据编码默认为Default
        /// </summary>
        public Encoding Encoding { get; set; } = Encoding.Default;

        /// <summary>
        /// 请求参数的数据类型（默认string）
        /// </summary>
        public PostDataType PostDataType { get; set; } = PostDataType.String;

        /// <summary>
        /// Post请求时要发送的字符串
        /// </summary>
        public string PostData { get; set; }

        /// <summary>
        /// Post数据
        /// </summary>
        public byte[] PostDataByte { get; set; }

        /// <summary>
        /// Cookie容器
        /// </summary>
        public CookieCollection CookieCollection { get; set; }

        /// <summary>
        /// 请求时的Cookie
        /// </summary>
        public string Cookie { get; set; }

        /// <summary>
        /// 上次访问地址
        /// </summary>
        public string Referer { get; set; }

        /// <summary>
        /// 证书绝对路径
        /// </summary>
        public string CerPath { get; set; }

        /// <summary>
        /// 设置代理对象，不想使用IE默认配置就设置为Null，而且不要设置ProxyIp
        /// </summary>
        public WebProxy WebProxy { get; set; }

        /// <summary>
        /// 是否全文小写(默认不转换)
        /// </summary>
        public Boolean IsToLower { get; set; } = false;

        /// <summary>
        /// 跳转页面，查询结果将是跳转后的页面，默认是不跳转
        /// </summary>
        public Boolean Allowautoredirect { get; set; } = false;

        /// <summary>
        /// 连接数限制(默认1024)
        /// </summary>
        public int Connectionlimit { get; set; } = 1024;

        /// <summary>
        /// 代理Proxy 服务器用户名
        /// </summary>
        public string ProxyUserName { get; set; }

        /// <summary>
        /// 代理 服务器密码
        /// </summary>
        public string ProxyPwd { get; set; }

        /// <summary>
        /// 代理 服务IP,如果要使用IE代理就设置为ieproxy
        /// </summary>
        public string ProxyIp { get; set; }

        /// <summary>
        /// 设置返回类型String和Byte
        /// </summary>
        public ResultType ResultType { get; set; } = ResultType.String;

        /// <summary>
        /// header对象
        /// </summary>
        public WebHeaderCollection Header { get; set; } = new WebHeaderCollection();

        /// <summary>
        //     获取或设置用于请求的 HTTP 版本。返回结果:用于请求的 HTTP 版本。默认为 System.Net.HttpVersion.Version11。
        /// </summary>
        public Version ProtocolVersion { get; set; }

        /// <summary>
        ///  获取或设置一个 System.Boolean 值，该值确定是否使用 100-Continue 行为。如果 POST 请求需要 100-Continue 响应，则为 true；否则为 false。默认值为 true。
        /// </summary>
        public Boolean Expect100Continue { get; set; } = true;

        /// <summary>
        /// 设置509证书集合
        /// </summary>
        public X509CertificateCollection ClentCertificates { get; set; }

        /// <summary>
        /// 设置或获取Post参数编码,默认的为Default编码
        /// </summary>
        public Encoding PostEncoding { get; set; }

        /// <summary>
        /// Cookie返回类型,默认返回字符串类型
        /// </summary>
        public CookieType ResponseCookieType { get; set; } = CookieType.String;

        /// <summary>
        /// 获取或设置请求的身份验证信息
        /// </summary>
        public ICredentials ICredentials { get; set; } = CredentialCache.DefaultCredentials;

        /// <summary>
        /// 设置授权token
        /// </summary>
        public string AuthorizationToken { get; set; }

        /// <summary>
        /// 设置请求将跟随的重定向的最大数目
        /// </summary>
        public int MaximumAutomaticRedirections { get; set; }

        /// <summary>
        /// 获取和设置IfModifiedSince，默认为当前日期和时间
        /// </summary>
        public DateTime? IfModifiedSince { get; set; }
    }

    /// <summary>
    /// 返回
    /// </summary>
    public class KogelResponse
    {
        /// <summary>
        /// Http请求返回的Cookie
        /// </summary>
        public string Cookie { get; set; }
        /// <summary>
        /// Cookie对象集合
        /// </summary>
        public CookieCollection CookieCollection { get; set; }

        /// <summary>
        /// 返回的String类型数据 
        /// </summary>
        public string Result { get; set; } = string.Empty;

        /// <summary>
        /// 返回的Byte数组
        /// </summary>
        public byte[] ResultByte { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public WebHeaderCollection Header { get; set; }

        /// <summary>
        /// 返回状态说明
        /// </summary>
        public string StatusDescription { get; set; }

        /// <summary>
        /// 返回状态码,默认为OK
        /// </summary>
        public HttpStatusCode StatusCode { get; set; }

        /// <summary>
        /// 最后访问的URl
        /// </summary>
        public string ResponseUrl { get; set; }

        /// <summary>
        /// 获取重定向的URl
        /// </summary>
        public string RedirectUrl
        {
            get
            {
                try
                {
                    if (Header != null && Header.Count > 0)
                    {
                        if (Header.AllKeys.Any(k => k.ToLower().Contains("location")))
                        {
                            string locationurl = Header["location"].ToString().ToLower();

                            if (!string.IsNullOrWhiteSpace(locationurl))
                            {
                                bool b = locationurl.StartsWith("http://") || locationurl.StartsWith("https://");
                                if (!b)
                                {
                                    locationurl = new Uri(new Uri(ResponseUrl), locationurl).AbsoluteUri;
                                }
                            }
                            return locationurl;
                        }
                    }
                }
                catch { }
                return string.Empty;
            }
        }
    }

    /// <summary>
    /// 返回类型
    /// </summary>
    public enum ResultType
    {
        /// <summary>
        /// 返回字符串类型
        /// </summary>
        String = 1,
        /// <summary>
        /// 返回流类型
        /// </summary>
        Byte
    }

    /// <summary>
    /// 请求参数的数据类型
    /// </summary>
    public enum PostDataType
    {
        /// <summary>
        /// 字符串类型
        /// </summary>
        String,

        /// <summary>
        /// Byte类型
        /// </summary>
        Byte,

        /// <summary>
        /// 传文件
        /// </summary>
        FilePath
    }

    /// <summary>
    /// Cookie返回类型
    /// </summary>
    public enum CookieType
    {
        /// <summary>
        /// 字符串类型
        /// </summary>
        String,

        /// <summary>
        /// cookie容器
        /// </summary>
        CookieCollection
    }
}
