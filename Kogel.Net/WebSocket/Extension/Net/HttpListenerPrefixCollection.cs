using System;
using System.Collections;
using System.Collections.Generic;

namespace Kogel.Net.WebSocket.Extension.Net
{
    /// <summary>
    /// 提供用于存储 <see cref="HttpListener"/> 类实例的 URI 前缀的集合
    /// </summary>
    public class HttpListenerPrefixCollection : ICollection<string>
    {
        private HttpListener _listener;
        private List<string> _prefixes;
        internal HttpListenerPrefixCollection(HttpListener listener)
        {
            _listener = listener;
            _prefixes = new List<string>();
        }

        /// <summary>
        /// 
        /// </summary>
        public int Count
        {
            get
            {
                return _prefixes.Count;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool IsSynchronized
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="uriPrefix"></param>
        public void Add(string uriPrefix)
        {
            _listener.CheckDisposed();

            HttpListenerPrefix.CheckPrefix(uriPrefix);

            if (_prefixes.Contains(uriPrefix))
                return;

            if (_listener.IsListening)
                EndPointManager.AddPrefix(uriPrefix, _listener);

            _prefixes.Add(uriPrefix);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Clear()
        {
            _listener.CheckDisposed();

            if (_listener.IsListening)
                EndPointManager.RemoveListener(_listener);

            _prefixes.Clear();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="uriPrefix"></param>
        /// <returns></returns>
        public bool Contains(string uriPrefix)
        {
            _listener.CheckDisposed();

            if (uriPrefix == null)
                throw new ArgumentNullException("uriPrefix");

            return _prefixes.Contains(uriPrefix);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="array"></param>
        /// <param name="offset"></param>
        public void CopyTo(string[] array, int offset)
        {
            _listener.CheckDisposed();

            _prefixes.CopyTo(array, offset);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerator<string> GetEnumerator()
        {
            return _prefixes.GetEnumerator();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="uriPrefix"></param>
        /// <returns></returns>
        public bool Remove(string uriPrefix)
        {
            _listener.CheckDisposed();

            if (uriPrefix == null)
                throw new ArgumentNullException("uriPrefix");

            if (!_prefixes.Contains(uriPrefix))
                return false;

            if (_listener.IsListening)
                EndPointManager.RemovePrefix(uriPrefix, _listener);

            return _prefixes.Remove(uriPrefix);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return _prefixes.GetEnumerator();
        }
    }
}
