using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kogel.Net.WebSocket.Extension
{
    /// <summary>
    /// 
    /// </summary>
    public class ErrorEventArgs : EventArgs
    {
        #region Private Fields

        private Exception _exception;
        private string _message;

        #endregion

        #region Internal Constructors

        internal ErrorEventArgs(string message)
          : this(message, null)
        {
        }

        internal ErrorEventArgs(string message, Exception exception)
        {
            _message = message;
            _exception = exception;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the exception that caused the error.
        /// </summary>
        /// <value>
        /// An <see cref="System.Exception"/> instance that represents the cause of
        /// the error if it is due to an exception; otherwise, <see langword="null"/>.
        /// </value>
        public Exception Exception
        {
            get
            {
                return _exception;
            }
        }

        /// <summary>
        /// Gets the error message.
        /// </summary>
        /// <value>
        /// A <see cref="string"/> that represents the error message.
        /// </value>
        public string Message
        {
            get
            {
                return _message;
            }
        }

        #endregion
    }
}
