using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kogel.Net.Http.Interfaces
{
    /// <summary>
    /// 文件连接
    /// </summary>
    public interface IFileClient
    {
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
        /// 文件上传
        /// </summary>
        /// <param name="url"></param>
        /// <param name="path"></param>
        /// <param name="authorizationToken"></param>
        /// <param name="authorizationMethod"></param>
        /// <param name="header"></param>
        /// <returns></returns>
        KogelResponse Upload(string url, string path, string authorizationToken = null, string authorizationMethod = "Bearer", IDictionary<string, string> header = null);
    }
}
