using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Kogel.Net.WebSocket.Extension.Net
{
    /// <summary>
    /// 处理HTTP请求出错时抛出的异常
    /// </summary>
    [Serializable]
    public class HttpListenerException : Win32Exception
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="serializationInfo"></param>
        /// <param name="streamingContext"></param>
        protected HttpListenerException(SerializationInfo serializationInfo, StreamingContext streamingContext) : base(serializationInfo, streamingContext)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        public HttpListenerException()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="errorCode"></param>
        public HttpListenerException(int errorCode) : base(errorCode)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="errorCode"></param>
        /// <param name="message"></param>
        public HttpListenerException(int errorCode, string message) : base(errorCode, message)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        public override int ErrorCode
        {
            get
            {
                return NativeErrorCode;
            }
        }
    }
}
