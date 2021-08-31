using Kogel.Net.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kogel.Net.Http.Interfaces
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

        /// <summary>
        /// 文件下载
        /// </summary>
        /// <param name="url"></param>
        /// <param name="path"></param>
        /// <param name="authorizationMethod"></param>
        /// <param name="authorizationToken"></param>
        /// <param name="header"></param>
        void Download(string url, string path, string authorizationToken = null, string authorizationMethod = "Bearer", IDictionary<string, string> header = null);

        /// <summary>
        /// 文件下载(异步)
        /// </summary>
        /// <param name="url"></param>
        /// <param name="path"></param>
        /// <param name="authorizationMethod"></param>
        /// <param name="authorizationToken"></param>
        /// <param name="header"></param>
        Task DownloadAsync(string url, string path, string authorizationToken = null, string authorizationMethod = "Bearer", IDictionary<string, string> header = null);

        /// <summary>
        /// 文件上传
        /// </summary>
        /// <param name="url"></param>
        /// <param name="path"></param>
        /// <param name="authorizationToken"></param>
        /// <param name="authorizationMethod"></param>
        /// <param name="header"></param>
        /// <returns></returns>
        KogelResponse Upload(string url, string path, string authorizationToken = null, string authorizationMethod = "Bearer", IDictionary<string, string> header = null);

        /// <summary>
        /// 文件上传(异步)
        /// </summary>
        /// <param name="url"></param>
        /// <param name="path"></param>
        /// <param name="authorizationToken"></param>
        /// <param name="authorizationMethod"></param>
        /// <param name="header"></param>
        /// <returns></returns>
        Task<KogelResponse> UploadAsync(string url, string path, string authorizationToken = null, string authorizationMethod = "Bearer", IDictionary<string, string> header = null);
    }
}
