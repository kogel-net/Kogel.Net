extern alias Kogel_Http;

using Kogel.Net.Http.Interfaces;
using Kogel_Http::Newtonsoft.Json;
using Kogel_Http::Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Kogel.Net.Http
{
    /// <summary>
    /// HTTP请求
    /// </summary>
    public partial class HttpClient : IHttpClient
    {
        /// <summary>
        /// Get请求
        /// </summary>
        /// <param name="url"></param>
        /// <param name="authorizationToken"></param>
        /// <param name="authorizationMethod"></param>
        /// <param name="header"></param>
        /// <returns></returns>
        public string Get(string url, string authorizationToken = null, string authorizationMethod = "Bearer", IDictionary<string, string> header = null)
        {
            HttpBase httpBase = new HttpBase();
            var webHeader = httpBase.SetHeader(authorizationToken, authorizationMethod, header);
            var response = httpBase.GetResponse(new KogelRequest
            {
                Method = "GET",
                Url = url,
                Header = webHeader
            });
            return response.Result;
        }

        /// <summary>
        /// Get请求
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="url"></param>
        /// <param name="authorizationToken"></param>
        /// <param name="authorizationMethod"></param>
        /// <param name="header"></param>
        /// <returns></returns>
        public TResult Get<TResult>(string url, string authorizationToken = null, string authorizationMethod = "Bearer", IDictionary<string, string> header = null)
        {
            HttpBase httpBase = new HttpBase();
            var webHeader = httpBase.SetHeader(authorizationToken, authorizationMethod, header);
            var response = httpBase.GetResponse(new KogelRequest
            {
                Method = "GET",
                Url = url,
                Header = webHeader
            });
            return GetResult<TResult>(response);
        }

        /// <summary>
        /// Get请求(异步)
        /// </summary>
        /// <param name="url"></param>
        /// <param name="authorizationToken"></param>
        /// <param name="authorizationMethod"></param>
        /// <param name="header"></param>
        /// <returns></returns>
        public async Task<string> GetAsync(string url, string authorizationToken, string authorizationMethod = "Bearer", IDictionary<string, string> header = null)
        {
            HttpBase httpBase = new HttpBase();
            var webHeader = httpBase.SetHeader(authorizationToken, authorizationMethod, header);
            var response = await httpBase.GetResponseAsync(new KogelRequest
            {
                Method = "GET",
                Url = url,
                Header = webHeader
            });
            return response.Result;
        }

        /// <summary>
        /// Get请求(异步)
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="url"></param>
        /// <param name="authorizationToken"></param>
        /// <param name="authorizationMethod"></param>
        /// <param name="header"></param>
        /// <returns></returns>
        public async Task<TResult> GetAsync<TResult>(string url, string authorizationToken = null, string authorizationMethod = "Bearer", IDictionary<string, string> header = null)
        {
            HttpBase httpBase = new HttpBase();
            var webHeader = httpBase.SetHeader(authorizationToken, authorizationMethod, header);
            var response = await httpBase.GetResponseAsync(new KogelRequest
            {
                Method = "GET",
                Url = url,
                Header = webHeader
            });
            return GetResult<TResult>(response);
        }

        /// <summary>
        /// Post请求
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url"></param>
        /// <param name="postData"></param>
        /// <param name="authorizationToken"></param>
        /// <param name="authorizationMethod"></param>
        /// <param name="header"></param>
        /// <returns></returns>
        public KogelResponse Post(string url, object postData, string authorizationToken = null, string authorizationMethod = "Bearer", IDictionary<string, string> header = null)
        {
            HttpBase httpBase = new HttpBase();
            var webHeader = httpBase.SetHeader(authorizationToken, authorizationMethod, header);
            var response = httpBase.GetResponse(new KogelRequest
            {
                Method = "POST",
                Url = url,
                PostDataType = PostDataType.Byte,
                PostDataByte = httpBase.GetPostDate(postData),
                Header = webHeader
            });
            return response;
        }

        /// <summary>
        /// Post请求
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="url"></param>
        /// <param name="postData"></param>
        /// <param name="authorizationToken"></param>
        /// <param name="authorizationMethod"></param>
        /// <param name="header"></param>
        /// <returns></returns>
        public TResult Post<TResult>(string url, object postData, string authorizationToken = null, string authorizationMethod = "Bearer", IDictionary<string, string> header = null)
        {
            HttpBase httpBase = new HttpBase();
            var webHeader = httpBase.SetHeader(authorizationToken, authorizationMethod, header);
            var response = httpBase.GetResponse(new KogelRequest
            {
                Method = "POST",
                Url = url,
                PostDataType = PostDataType.Byte,
                PostDataByte = httpBase.GetPostDate(postData),
                Header = webHeader
            });
            return GetResult<TResult>(response);
        }

        /// <summary>
        /// Post请求(异步)
        /// </summary>
        /// <param name="url"></param>
        /// <param name="postData"></param>
        /// <param name="authorizationToken"></param>
        /// <param name="authorizationMethod"></param>
        /// <param name="header"></param>
        /// <returns></returns>
        public async Task<KogelResponse> PostAsync(string url, object postData, string authorizationToken = null, string authorizationMethod = "Bearer", IDictionary<string, string> header = null)
        {
            HttpBase httpBase = new HttpBase();
            var webHeader = httpBase.SetHeader(authorizationToken, authorizationMethod, header);
            var response = await httpBase.GetResponseAsync(new KogelRequest
            {
                Method = "POST",
                Url = url,
                PostDataType = PostDataType.Byte,
                PostDataByte = httpBase.GetPostDate(postData),
                Header = webHeader
            });
            return response;
        }

        /// <summary>
        /// Post请求(异步)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url"></param>
        /// <param name="postData"></param>
        /// <param name="authorizationToken"></param>
        /// <param name="authorizationMethod"></param>
        /// <param name="header"></param>
        /// <returns></returns>
        public async Task<TResult> PostAsync<TResult>(string url, object postData, string authorizationToken = null, string authorizationMethod = "Bearer", IDictionary<string, string> header = null)
        {
            HttpBase httpBase = new HttpBase();
            var webHeader = httpBase.SetHeader(authorizationToken, authorizationMethod, header);
            var response = await httpBase.GetResponseAsync(new KogelRequest
            {
                Method = "POST",
                Url = url,
                PostDataType = PostDataType.Byte,
                PostDataByte = httpBase.GetPostDate(postData),
                Header = webHeader
            });
            return GetResult<TResult>(response);
        }

        /// <summary>
        /// Post请求
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public KogelResponse Request(KogelRequest request)
        {
            HttpBase httpBase = new HttpBase();
            var response = httpBase.GetResponse(request);
            return response;
        }

        /// <summary>
        /// Post请求(异步)
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<KogelResponse> RequestAsync(KogelRequest request)
        {
            HttpBase httpBase = new HttpBase();
            var response = await httpBase.GetResponseAsync(request);
            return response;
        }

        /// <summary>
        /// 获取返回（带转换）
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="response"></param>
        /// <returns></returns>
        protected virtual TResult GetResult<TResult>(KogelResponse response)
        {
            if (response.StatusCode == HttpStatusCode.OK && !string.IsNullOrEmpty(response.Result))
                return (TResult)JsonConvert.DeserializeObject(response.Result, typeof(TResult), CamelCaseOnlyConverter.Settings);
            else
                return default;
        }

        /// <summary>
        /// 文件下载
        /// </summary>
        /// <param name="url"></param>
        /// <param name="path"></param>
        /// <param name="authorizationMethod"></param>
        /// <param name="authorizationToken"></param>
        /// <param name="header"></param>
        public void Download(string url, string path, string authorizationToken = null, string authorizationMethod = "Bearer", IDictionary<string, string> header = null)
        {
            StringBuilder progress = new StringBuilder();
            HttpBase httpBase = new HttpBase();
            KogelRequest request = new KogelRequest()
            {
                Url = url,
                ContentType = "application/octet-stream"
            };
            try
            {
                HttpBase.Aop.InvokeExecuting(request);
                //获取上次下载文件的位置
                long lStartPos = httpBase.GetStartPost(path, out FileStream fileStream);
                try
                {
                    //准备参数
                    httpBase.SetRequest(request);
                    //设置range值
                    httpBase.httpRequest.AddRange((int)lStartPos);
                }
                catch (Exception ex)
                {
                    throw new Exception("配置参数时出错：" + ex.Message);
                }
                try
                {
                    //请求数据
                    httpBase.httpResponse = (HttpWebResponse)httpBase.httpRequest.GetResponse();
                    using (Stream netStream = httpBase.httpResponse.GetResponseStream())
                    {
                        long totalSize = httpBase.httpResponse.ContentLength;
                        long hasDownSize = 0;
                        byte[] nbytes = new byte[512];//521,2048 etc
                        int nReadSize = 0;
                        nReadSize = netStream.Read(nbytes, 0, nbytes.Length);
                        while (nReadSize > 0)
                        {
                            progress.Clear();
                            fileStream.Write(nbytes, 0, nReadSize);
                            nReadSize = netStream.Read(nbytes, 0, nbytes.Length);
                            hasDownSize += nReadSize;
                        }
                        fileStream.Close();
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            finally
            {
                HttpBase.Aop.InvokeExecuted(request);
            }
        }

        /// <summary>
        /// 文件下载(异步)
        /// </summary>
        /// <param name="url"></param>
        /// <param name="path"></param>
        /// <param name="authorizationMethod"></param>
        /// <param name="authorizationToken"></param>
        /// <param name="header"></param>
        public async Task DownloadAsync(string url, string path, string authorizationToken = null, string authorizationMethod = "Bearer", IDictionary<string, string> header = null)
        {
            StringBuilder progress = new StringBuilder();
            HttpBase httpBase = new HttpBase();
            KogelRequest request = new KogelRequest()
            {
                Url = url,
                ContentType = "application/octet-stream"
            };
            try
            {
                HttpBase.Aop.InvokeExecuting(request);
                //获取上次下载文件的位置
                long lStartPos = httpBase.GetStartPost(path, out FileStream fileStream);
                try
                {
                    //准备参数
                    httpBase.SetRequest(request);
                    //设置range值
                    httpBase.httpRequest.AddRange((int)lStartPos);
                }
                catch (Exception ex)
                {
                    throw new Exception("配置参数时出错：" + ex.Message);
                }
                try
                {
                    //请求数据
                    httpBase.httpResponse = (HttpWebResponse)await httpBase.httpRequest.GetResponseAsync();
                    using (Stream netStream = httpBase.httpResponse.GetResponseStream())
                    {
                        long totalSize = httpBase.httpResponse.ContentLength;
                        long hasDownSize = 0;
                        byte[] nbytes = new byte[512];//521,2048 etc
                        int nReadSize = 0;
                        nReadSize = netStream.Read(nbytes, 0, nbytes.Length);
                        while (nReadSize > 0)
                        {
                            progress.Clear();
                            fileStream.Write(nbytes, 0, nReadSize);
                            nReadSize = netStream.Read(nbytes, 0, nbytes.Length);
                            hasDownSize += nReadSize;
                        }
                        fileStream.Close();
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            finally
            {
                HttpBase.Aop.InvokeExecuted(request);
            }
        }

        /// <summary>
        /// 文件上传
        /// </summary>
        /// <param name="url"></param>
        /// <param name="path"></param>
        /// <param name="authorizationToken"></param>
        /// <param name="authorizationMethod"></param>
        /// <param name="header"></param>
        /// <returns></returns>
        public KogelResponse Upload(string url, string path, string authorizationToken = null, string authorizationMethod = "Bearer", IDictionary<string, string> header = null)
        {
            HttpBase httpBase = new HttpBase();
            var webHeader = httpBase.SetHeader(authorizationToken, authorizationMethod, header);
            var response = httpBase.GetResponse(new KogelRequest
            {
                Method = "POST",
                Url = url,
                PostDataType = PostDataType.FilePath,
                PostData = path,
                Header = webHeader
            });
            return response;
        }

        /// <summary>
        /// 文件上传
        /// </summary>
        /// <param name="url"></param>
        /// <param name="path"></param>
        /// <param name="authorizationToken"></param>
        /// <param name="authorizationMethod"></param>
        /// <param name="header"></param>
        /// <returns></returns>
        public async Task<KogelResponse> UploadAsync(string url, string path, string authorizationToken = null, string authorizationMethod = "Bearer", IDictionary<string, string> header = null)
        {
            HttpBase httpBase = new HttpBase();
            var webHeader = httpBase.SetHeader(authorizationToken, authorizationMethod, header);
            var response = await httpBase.GetResponseAsync(new KogelRequest
            {
                Method = "POST",
                Url = url,
                PostDataType = PostDataType.FilePath,
                PostData = path,
                Header = webHeader
            });
            return response;
        }
    }
}
