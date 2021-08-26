extern alias Kogel_Http;

using Kogel_Http::Newtonsoft.Json;
using Kogel_Http::Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Kogel.Net.Extension
{
    /// <summary>
    /// 
    /// </summary>
    public class HttpClient : IHttpClient
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
            var webHeader = SetHeader(authorizationToken, authorizationMethod, header);
            HttpBase httpBase = new HttpBase();
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
            var webHeader = SetHeader(authorizationToken, authorizationMethod, header);
            HttpBase httpBase = new HttpBase();
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
            var webHeader = SetHeader(authorizationToken, authorizationMethod, header);
            HttpBase httpBase = new HttpBase();
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
            var webHeader = SetHeader(authorizationToken, authorizationMethod, header);
            HttpBase httpBase = new HttpBase();
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
            var webHeader = SetHeader(authorizationToken, authorizationMethod, header);
            HttpBase httpBase = new HttpBase();
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
            var webHeader = SetHeader(authorizationToken, authorizationMethod, header);
            HttpBase httpBase = new HttpBase();
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
            var webHeader = SetHeader(authorizationToken, authorizationMethod, header);
            HttpBase httpBase = new HttpBase();
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
            var webHeader = SetHeader(authorizationToken, authorizationMethod, header);
            HttpBase httpBase = new HttpBase();
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
        /// 设置头部信息
        /// </summary>
        /// <param name="authorizationToken"></param>
        /// <param name="authorizationMethod"></param>
        /// <param name="header"></param>
        /// <returns></returns>
        private WebHeaderCollection SetHeader(string authorizationToken = null, string authorizationMethod = "Bearer", IDictionary<string, string> header = null)
        {
            WebHeaderCollection webHeader = new WebHeaderCollection();
            //设置身份授权Token
            if (!string.IsNullOrEmpty(authorizationToken))
            {
                if (!string.IsNullOrEmpty(authorizationMethod))
                    webHeader.Add("Authorization", $"{authorizationMethod} {authorizationToken}");
                else
                    webHeader.Add("Authorization", authorizationToken);
            }
            //设置请求头
            if (header != null)
            {
                foreach (var item in header)
                {
                    webHeader.Add(item.Key, item.Value);
                }
            }
            return webHeader;
        }

        /// <summary>
        /// 获取返回（带转换）
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="response"></param>
        /// <returns></returns>
        protected virtual TResult GetResult<TResult>(KogelResponse response)
        {
            if (response.StatusCode == HttpStatusCode.OK)
                return (TResult)JsonConvert.DeserializeObject(response.Result, typeof(TResult), CamelCaseOnlyConverter.Settings);
            else
                return default;
        }
    }
}
