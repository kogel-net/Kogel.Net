using Kogel.Net.WebSocket.Extension.Net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Kogel.Net.WebSocket.Extension
{
    /// <summary>
    /// 表示 HTTP 请求事件的事件数据
    /// </summary>
    public class HttpRequestEventArgs : EventArgs
    {
        private HttpListenerContext _context;
        private string _docRootPath;
        internal HttpRequestEventArgs( HttpListenerContext context, string documentRootPath)
        {
            _context = context;
            _docRootPath = documentRootPath;
        }

     /// <summary>
     /// 
     /// </summary>
        public HttpListenerRequest Request
        {
            get
            {
                return _context.Request;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public HttpListenerResponse Response
        {
            get
            {
                return _context.Response;
            }
        }

      /// <summary>
      /// 
      /// </summary>
        public IPrincipal User
        {
            get
            {
                return _context.User;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="childPath"></param>
        /// <returns></returns>
        private string CreateFilePath(string childPath)
        {
            childPath = childPath.TrimStart('/', '\\');

            return new StringBuilder(_docRootPath, 32)
                   .AppendFormat("/{0}", childPath)
                   .ToString()
                   .Replace('\\', '/');
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="contents"></param>
        /// <returns></returns>
        private static bool _TryReadFile(string path, out byte[] contents)
        {
            contents = null;

            if (!File.Exists(path))
                return false;

            try
            {
                contents = File.ReadAllBytes(path);
            }
            catch
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 从文档文件夹中读取指定的文件
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public byte[] ReadFile(string path)
        {
            if (path == null)
                throw new ArgumentNullException("path");

            if (path.Length == 0)
                throw new ArgumentException("An empty string.", "path");

            if (path.IndexOf("..") > -1)
                throw new ArgumentException("It contains '..'.", "path");

            path = CreateFilePath(path);
            byte[] contents;

            _TryReadFile(path, out contents);

            return contents;
        }

       /// <summary>
       /// 
       /// </summary>
       /// <param name="path"></param>
       /// <param name="contents"></param>
       /// <returns></returns>
        public bool TryReadFile(string path, out byte[] contents)
        {
            if (path == null)
                throw new ArgumentNullException("path");

            if (path.Length == 0)
                throw new ArgumentException("An empty string.", "path");

            if (path.IndexOf("..") > -1)
                throw new ArgumentException("It contains '..'.", "path");

            path = CreateFilePath(path);

            return _TryReadFile(path, out contents);
        }  
    }
}
