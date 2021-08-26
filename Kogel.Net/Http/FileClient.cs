using Kogel.Net.Http.Interfaces;
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
    /// 文件连接
    /// </summary>
    public class FileClient : IFileClient
    {
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
                long lStartPos = GetStartPost(path, out FileStream fileStream);
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
        /// 获取上次下载文件的位置（新增文件为0）
        /// </summary>
        /// <param name="path"></param>
        /// <param name="fileStream"></param>
        /// <returns></returns>
        private long GetStartPost(string path, out FileStream fileStream)
        {
            //打开上次下载的文件或新建文件 
            long lStartPos = 0;
            //另外如果文件已经下载完毕，就不需要再断点续传了，不然请求的range 会不合法会抛出异常。
            if (File.Exists(path))
            {
                fileStream = File.OpenWrite(path);
                lStartPos = fileStream.Length;
                fileStream.Seek(lStartPos, SeekOrigin.Current); //移动文件流中的当前指针 
            }
            else
            {
                fileStream = new FileStream(path, FileMode.Create);
                lStartPos = 0;
            }
            return lStartPos;
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
    }
}
