using Kogel.Net.Extension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kogel.Net
{
    /// <summary>
    /// 
    /// </summary>
    public interface IHttpClient
    {
        /// <summary>
        /// Get请求
        /// </summary>
        /// <param name="url"></param>
        /// <param name="authorizationToken"></param>
        /// <param name="authorizationMethod"></param>
        /// <param name="header"></param>
        /// <returns></returns>
        string Get(string url, string authorizationToken = null, string authorizationMethod = "Bearer", IDictionary<string, string> header = null);

        /// <summary>
        /// Get请求
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="url"></param>
        /// <param name="authorizationToken"></param>
        /// <param name="authorizationMethod"></param>
        /// <param name="header"></param>
        /// <returns></returns>
        TResult Get<TResult>(string url, string authorizationToken = null, string authorizationMethod = "Bearer", IDictionary<string, string> header = null);

        /// <summary>
        /// Get请求(异步)
        /// </summary>
        /// <param name="url"></param>
        /// <param name="authorizationToken"></param>
        /// <param name="authorizationMethod"></param>
        /// <param name="header"></param>
        /// <returns></returns>
        Task<string> GetAsync(string url, string authorizationToken = null, string authorizationMethod = "Bearer", IDictionary<string, string> header = null);

        /// <summary>
        /// Get请求(异步)
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="url"></param>
        /// <param name="authorizationToken"></param>
        /// <param name="authorizationMethod"></param>
        /// <param name="header"></param>
        /// <returns></returns>
        Task<TResult> GetAsync<TResult>(string url, string authorizationToken = null, string authorizationMethod = "Bearer", IDictionary<string, string> header = null);

        /// <summary>
        /// Post请求
        /// </summary>
        /// <param name="url"></param>
        /// <param name="postData"></param>
        /// <param name="authorizationToken"></param>
        /// <param name="authorizationMethod"></param>
        /// <param name="header"></param>
        /// <returns></returns>
        KogelResponse Post(string url, object postData, string authorizationToken = null, string authorizationMethod = "Bearer", IDictionary<string, string> header = null);

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
        TResult Post<TResult>(string url, object postData, string authorizationToken = null, string authorizationMethod = "Bearer", IDictionary<string, string> header = null);

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
        Task<KogelResponse> PostAsync(string url, object postData, string authorizationToken = null, string authorizationMethod = "Bearer", IDictionary<string, string> header = null);

        /// <summary>
        /// Post请求(异步)
        /// </summary>
        /// <param name="url"></param>
        /// <param name="postData"></param>
        /// <param name="authorizationToken"></param>
        /// <param name="authorizationMethod"></param>
        /// <param name="header"></param>
        /// <returns></returns>
        Task<TResult> PostAsync<TResult>(string url, object postData, string authorizationToken = null, string authorizationMethod = "Bearer", IDictionary<string, string> header = null);

        /// <summary>
        /// 自定义请求
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        KogelResponse Request(KogelRequest request);

        /// <summary>
        /// 自定义请求(异步)
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<KogelResponse> RequestAsync(KogelRequest request);
    }
}
