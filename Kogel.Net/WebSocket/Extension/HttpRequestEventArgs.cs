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
        #region Private Fields

        private HttpListenerContext _context;
        private string _docRootPath;

        #endregion

        #region Internal Constructors

        internal HttpRequestEventArgs(
          HttpListenerContext context, string documentRootPath
        )
        {
            _context = context;
            _docRootPath = documentRootPath;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the request data sent from a client.
        /// </summary>
        /// <value>
        /// A <see cref="HttpListenerRequest"/> that provides the methods and
        /// properties for the request data.
        /// </value>
        public HttpListenerRequest Request
        {
            get
            {
                return _context.Request;
            }
        }

        /// <summary>
        /// Gets the response data to return to the client.
        /// </summary>
        /// <value>
        /// A <see cref="HttpListenerResponse"/> that provides the methods and
        /// properties for the response data.
        /// </value>
        public HttpListenerResponse Response
        {
            get
            {
                return _context.Response;
            }
        }

        /// <summary>
        /// Gets the information for the client.
        /// </summary>
        /// <value>
        ///   <para>
        ///   A <see cref="IPrincipal"/> instance or <see langword="null"/>
        ///   if not authenticated.
        ///   </para>
        ///   <para>
        ///   That instance describes the identity, authentication scheme,
        ///   and security roles for the client.
        ///   </para>
        /// </value>
        public IPrincipal User
        {
            get
            {
                return _context.User;
            }
        }

        #endregion

        #region Private Methods

        private string CreateFilePath(string childPath)
        {
            childPath = childPath.TrimStart('/', '\\');

            return new StringBuilder(_docRootPath, 32)
                   .AppendFormat("/{0}", childPath)
                   .ToString()
                   .Replace('\\', '/');
        }

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

        #endregion

        #region Public Methods

        /// <summary>
        /// Reads the specified file from the document folder of the
        /// <see cref="HttpServer"/> class.
        /// </summary>
        /// <returns>
        ///   <para>
        ///   An array of <see cref="byte"/> or <see langword="null"/>
        ///   if it fails.
        ///   </para>
        ///   <para>
        ///   That array receives the contents of the file.
        ///   </para>
        /// </returns>
        /// <param name="path">
        /// A <see cref="string"/> that specifies a virtual path to
        /// find the file from the document folder.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="path"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///   <para>
        ///   <paramref name="path"/> is an empty string.
        ///   </para>
        ///   <para>
        ///   -or-
        ///   </para>
        ///   <para>
        ///   <paramref name="path"/> contains "..".
        ///   </para>
        /// </exception>
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
        /// Tries to read the specified file from the document folder of
        /// the <see cref="HttpServer"/> class.
        /// </summary>
        /// <returns>
        /// <c>true</c> if it succeeds to read; otherwise, <c>false</c>.
        /// </returns>
        /// <param name="path">
        /// A <see cref="string"/> that specifies a virtual path to find
        /// the file from the document folder.
        /// </param>
        /// <param name="contents">
        ///   <para>
        ///   When this method returns, an array of <see cref="byte"/> or
        ///   <see langword="null"/> if it fails.
        ///   </para>
        ///   <para>
        ///   That array receives the contents of the file.
        ///   </para>
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="path"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///   <para>
        ///   <paramref name="path"/> is an empty string.
        ///   </para>
        ///   <para>
        ///   -or-
        ///   </para>
        ///   <para>
        ///   <paramref name="path"/> contains "..".
        ///   </para>
        /// </exception>
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

        #endregion
    }
}
