using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kogel.Net.WebSocket.Extension
{
    /// <summary>
    /// 异常事件
    /// </summary>
    public class ErrorEventArgs : EventArgs
    {
        private Exception _exception;
        private string _message;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        internal ErrorEventArgs(string message): this(message, null)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        internal ErrorEventArgs(string message, Exception exception)
        {
            _message = message;
            _exception = exception;
        }

        /// <summary>
        /// 获取导致错误的异常
        /// </summary>
        public Exception Exception
        {
            get
            {
                return _exception;
            }
        }

        /// <summary>
        /// 获取错误信息
        /// </summary>
        public string Message
        {
            get
            {
                return _message;
            }
        }
    }
}
