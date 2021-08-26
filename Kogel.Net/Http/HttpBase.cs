extern alias Kogel_Http;
using Kogel_Http::Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Kogel.Net.Http
{
    /// <summary>
    /// 
    /// </summary>
    public class HttpBase
    {
        private Encoding encoding = Encoding.Default;
        private Encoding postencoding = Encoding.Default;
        private HttpWebRequest httpRequest = null;
        private HttpWebResponse httpResponse = null;
        #region Aop
        public static AopProvider Aop { get => AopProvider.Get(); }
        #endregion

        /// <summary>
        /// 生成请求数据
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public byte[] GetPostDate(object param)
        {
            var jsonData = JsonConvert.SerializeObject(param);
            return Encoding.UTF8.GetBytes(jsonData);
        }

        /// <summary>
        /// 为string格式的字符串生成请求数据（XML可用）
        /// </summary>
        /// <returns></returns>
        public byte[] GetPostDateOther(string param)
        {
            return Encoding.UTF8.GetBytes(param);
        }

        /// <summary>
        /// 获取数据的并解析的方法
        /// </summary>
        /// <param name="item"></param>
        /// <param name="result"></param>
        private void GetData(KogelRequest request, KogelResponse result)
        {
            #region base
            //获取StatusCode
            result.StatusCode = httpResponse.StatusCode;
            //获取StatusDescription
            result.StatusDescription = httpResponse.StatusDescription;
            //获取Headers
            result.Header = httpResponse.Headers;
            //获取最后访问的URl
            result.ResponseUrl = httpResponse.ResponseUri.ToString();
            //获取CookieCollection
            if (httpResponse.Cookies != null) result.CookieCollection = httpResponse.Cookies;
            //获取set-cookie
            if (httpResponse.Headers["set-cookie"] != null) result.Cookie = httpResponse.Headers["set-cookie"];
            #endregion

            #region byte
            //处理网页Byte
            byte[] ResponseByte = GetByte();
            #endregion

            #region Html
            if (ResponseByte != null & ResponseByte.Length > 0)
            {
                //设置编码
                SetEncoding(request, result, ResponseByte);
                //得到返回的HTML
                result.Result = encoding.GetString(ResponseByte);
            }
            else
            {
                //没有返回任何Html代码
                result.Result = string.Empty;
            }
            #endregion
        }

        /// <summary>
        /// 设置编码
        /// </summary>
        /// <param name="item">HttpItem</param>
        /// <param name="result">HttpResult</param>
        /// <param name="ResponseByte">byte[]</param>
        private void SetEncoding(KogelRequest request, KogelResponse result, byte[] ResponseByte)
        {
            //是否返回Byte类型数据
            if (request.ResultType == ResultType.Byte) result.ResultByte = ResponseByte;
            //从这里开始我们要无视编码了
            if (encoding == null)
            {
                Match meta = Regex.Match(Encoding.Default.GetString(ResponseByte), "<meta[^<]*charset=([^<]*)[\"']", RegexOptions.IgnoreCase);
                string c = string.Empty;
                if (meta != null && meta.Groups.Count > 0)
                {
                    c = meta.Groups[1].Value.ToLower().Trim();
                }
                if (c.Length > 2)
                {
                    try
                    {
                        encoding = Encoding.GetEncoding(c.Replace("\"", string.Empty).Replace("'", "").Replace(";", "").Replace("iso-8859-1", "gbk").Trim());
                    }
                    catch
                    {
                        if (string.IsNullOrEmpty(httpResponse.CharacterSet))
                        {
                            encoding = Encoding.UTF8;
                        }
                        else
                        {
                            encoding = Encoding.GetEncoding(httpResponse.CharacterSet);
                        }
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(httpResponse.CharacterSet))
                    {
                        encoding = Encoding.UTF8;
                    }
                    else
                    {
                        encoding = Encoding.GetEncoding(httpResponse.CharacterSet);
                    }
                }
            }
        }

        /// <summary>
        /// 获取响应流
        /// </summary>
        /// <returns></returns>
        private byte[] GetByte()
        {
            byte[] ResponseByte = null;
            using (MemoryStream _stream = new MemoryStream())
            {
                //GZIIP处理
                if (httpResponse.ContentEncoding != null && httpResponse.ContentEncoding.Equals("gzip", StringComparison.InvariantCultureIgnoreCase))
                {
                    //开始读取流并设置编码方式
                    new GZipStream(httpResponse.GetResponseStream(), CompressionMode.Decompress).CopyTo(_stream, 10240);
                }
                else
                {
                    //开始读取流并设置编码方式
                    httpResponse.GetResponseStream().CopyTo(_stream, 10240);
                }
                //获取Byte
                ResponseByte = _stream.ToArray();
            }
            return ResponseByte;
        }

        /// <summary>
        /// 设置请求参数
        /// </summary>
        ///<param name="request"></param>
        private void SetRequest(KogelRequest request)
        {
            // 验证证书
            SetCer(request);
            //设置Header参数
            if (request.Header != null && request.Header.Count > 0)
            {
                foreach (string key in request.Header.AllKeys)
                {
                    httpRequest.Headers.Add(key, request.Header[key]);
                }
            }
            SetProxy(request);
            if (request.ProtocolVersion != null) request.ProtocolVersion = request.ProtocolVersion;
            httpRequest.ServicePoint.Expect100Continue = request.Expect100Continue;
            httpRequest.Method = request.Method;
            httpRequest.Timeout = request.Timeout;
            httpRequest.KeepAlive = request.KeepAlive;
            httpRequest.ReadWriteTimeout = request.WriteTimeout;
            if (!string.IsNullOrWhiteSpace(request.Host))
            {
                request.Host = request.Host;
            }
            if (request.IfModifiedSince != null) request.IfModifiedSince = Convert.ToDateTime(request.IfModifiedSince);
            httpRequest.Accept = request.Accept;
            //返回类型
            httpRequest.ContentType = request.ContentType;
            httpRequest.UserAgent = request.UserAgent;
            // 编码
            encoding = request.Encoding;
            //设置安全凭证
            httpRequest.Credentials = request.ICredentials;
            SetCookie(request);
            httpRequest.Referer = request.Referer;
            httpRequest.AllowAutoRedirect = request.Allowautoredirect;
            if (request.MaximumAutomaticRedirections > 0)
            {
                httpRequest.MaximumAutomaticRedirections = request.MaximumAutomaticRedirections;
            }
            //设置Post数据
            SetPostData(request);
            //设置最大连接
            if (request.Connectionlimit > 0) httpRequest.ServicePoint.ConnectionLimit = request.Connectionlimit;
        }

        /// <summary>
        /// 设置证书
        /// </summary>
        /// <param name="request"></param>
        private void SetCer(KogelRequest request)
        {
            if (!string.IsNullOrWhiteSpace(request.CerPath))
            {
                //设置验证证书回调
                ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
                httpRequest = (HttpWebRequest)WebRequest.Create(request.Url);
                SetCerList(request);
                //将证书添加到请求里
                httpRequest.ClientCertificates.Add(new X509Certificate(request.CerPath));
            }
            else
            {
                //初始化对像，并设置请求的URL地址
                httpRequest = (HttpWebRequest)WebRequest.Create(request.Url);
                SetCerList(request);
            }
        }

        /// <summary>
        /// 设置多个证书
        /// </summary>
        /// <param name="request"></param>
        private void SetCerList(KogelRequest request)
        {
            if (request.ClentCertificates != null && request.ClentCertificates.Count > 0)
            {
                foreach (X509Certificate cert in request.ClentCertificates)
                {
                    httpRequest.ClientCertificates.Add(cert);
                }
            }
        }

        /// <summary>
        /// 设置Cookie
        /// </summary>
        /// <param name="request"></param>
        private void SetCookie(KogelRequest request)
        {
            if (!string.IsNullOrEmpty(request.Cookie)) httpRequest.Headers[HttpRequestHeader.Cookie] = request.Cookie;
            //设置CookieCollection
            if (request.ResponseCookieType == CookieType.CookieCollection)
            {
                httpRequest.CookieContainer = new CookieContainer();
                if (request.CookieCollection != null && request.CookieCollection.Count > 0)
                    httpRequest.CookieContainer.Add(request.CookieCollection);
            }
        }

        /// <summary>
        /// 设置Post数据
        /// </summary>
        /// <param name="request"></param>
        private void SetPostData(KogelRequest request)
        {
            //验证在得到结果时是否有传入数据
            if (!request.Method.Trim().ToLower().Contains("get"))
            {
                if (request.PostEncoding != null)
                {
                    postencoding = request.PostEncoding;
                }
                byte[] buffer = null;
                //写入Byte类型
                if (request.PostDataType == PostDataType.Byte && request.PostDataByte != null && request.PostDataByte.Length > 0)
                {
                    //验证在得到结果时是否有传入数据
                    buffer = request.PostDataByte;
                }//写入文件
                else if (request.PostDataType == PostDataType.FilePath && !string.IsNullOrWhiteSpace(request.PostData))
                {
                    StreamReader r = new StreamReader(request.PostData, postencoding);
                    buffer = postencoding.GetBytes(r.ReadToEnd());
                    r.Close();
                } //写入字符串
                else if (!string.IsNullOrWhiteSpace(request.PostData))
                {
                    buffer = postencoding.GetBytes(request.PostData);
                }
                if (buffer != null)
                {
                    httpRequest.ContentLength = buffer.Length;
                    httpRequest.GetRequestStream().Write(buffer, 0, buffer.Length);
                }
            }
        }

        /// <summary>
        /// 设置代理
        /// </summary>
        /// <param name="request"></param>
        private void SetProxy(KogelRequest request)
        {
            bool isIeProxy = false;
            if (!string.IsNullOrWhiteSpace(request.ProxyIp))
            {
                isIeProxy = request.ProxyIp.ToLower().Contains("ieproxy");
            }
            if (!string.IsNullOrWhiteSpace(request.ProxyIp) && !isIeProxy)
            {
                //设置代理服务器
                if (request.ProxyIp.Contains(":"))
                {
                    string[] plist = request.ProxyIp.Split(':');
                    WebProxy myProxy = new WebProxy(plist[0].Trim(), Convert.ToInt32(plist[1].Trim()));
                    myProxy.Credentials = new NetworkCredential(request.ProxyUserName, request.ProxyPwd);
                    httpRequest.Proxy = myProxy;
                }
                else
                {
                    WebProxy myProxy = new WebProxy(request.ProxyIp, false);
                    myProxy.Credentials = new NetworkCredential(request.ProxyUserName, request.ProxyPwd);
                    httpRequest.Proxy = myProxy;
                }
            }
            else if (isIeProxy)
            {
            }
            else
            {
                httpRequest.Proxy = request.WebProxy;
            }
        }

        /// <summary>
        /// 回调验证证书问题
        /// </summary>
        /// <param name="sender">流对象</param>
        /// <param name="certificate">证书</param>
        /// <param name="chain">X509Chain</param>
        /// <param name="errors">SslPolicyErrors</param>
        /// <returns>bool</returns>
        private bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            return true;
        }

        /// <summary>
        /// 获取响应
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public KogelResponse GetResponse(KogelRequest request)
        {
            KogelResponse response = new KogelResponse();
            try
            {
                Aop.InvokeExecuting(request);
                try
                {
                    //准备参数
                    SetRequest(request);
                }
                catch (Exception ex)
                {
                    //配置参数时出错
                    return new KogelResponse()
                    {
                        Cookie = string.Empty,
                        Header = null,
                        Result = ex.Message,
                        StatusDescription = "配置参数时出错：" + ex.Message
                    };
                }
                try
                {
                    //请求数据
                    using (httpResponse = (HttpWebResponse)httpRequest.GetResponse())
                    {
                        GetData(request, response);
                    }
                }
                catch (WebException ex)
                {
                    if (ex.Response != null)
                    {
                        using (httpResponse = (HttpWebResponse)ex.Response)
                        {
                            GetData(request, response);
                        }
                    }
                    else
                    {
                        response.Result = ex.Message;
                    }
                }
                catch (Exception ex)
                {
                    response.Result = ex.Message;
                }
                if (request.IsToLower) response.Result = response.Result.ToLower();
            }
            finally
            {
                Aop.InvokeExecuted(request);
            }
            return response;
        }

        /// <summary>
        /// 获取响应
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<KogelResponse> GetResponseAsync(KogelRequest request)
        {
            KogelResponse response = new KogelResponse();
            try
            {
                Aop.InvokeExecuting(request);
                try
                {
                    //准备参数
                    SetRequest(request);
                }
                catch (Exception ex)
                {
                    //配置参数时出错
                    return new KogelResponse()
                    {
                        Cookie = string.Empty,
                        Header = null,
                        Result = ex.Message,
                        StatusDescription = "配置参数时出错：" + ex.Message
                    };
                }
                try
                {
                    //请求数据
                    using (httpResponse = (HttpWebResponse)await httpRequest.GetResponseAsync())
                    {
                        GetData(request, response);
                    }
                }
                catch (WebException ex)
                {
                    if (ex.Response != null)
                    {
                        using (httpResponse = (HttpWebResponse)ex.Response)
                        {
                            GetData(request, response);
                        }
                    }
                    else
                    {
                        response.Result = ex.Message;
                    }
                }
                catch (Exception ex)
                {
                    response.Result = ex.Message;
                }
                if (request.IsToLower) response.Result = response.Result.ToLower();
            }
            finally
            {
                Aop.InvokeExecuted(request);
            }
            return response;
        }
    }
}
