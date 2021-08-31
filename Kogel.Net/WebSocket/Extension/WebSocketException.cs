﻿using Kogel.Net.WebSocket.Enums;
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
    public class WebSocketException : Exception
    {
        #region Private Fields

        private CloseStatusCode _code;

        #endregion

        #region Internal Constructors

        internal WebSocketException()
          : this(CloseStatusCode.Abnormal, null, null)
        {
        }

        internal WebSocketException(Exception innerException)
          : this(CloseStatusCode.Abnormal, null, innerException)
        {
        }

        internal WebSocketException(string message)
          : this(CloseStatusCode.Abnormal, message, null)
        {
        }

        internal WebSocketException(CloseStatusCode code)
          : this(code, null, null)
        {
        }

        internal WebSocketException(string message, Exception innerException)
          : this(CloseStatusCode.Abnormal, message, innerException)
        {
        }

        internal WebSocketException(CloseStatusCode code, Exception innerException)
          : this(code, null, innerException)
        {
        }

        internal WebSocketException(CloseStatusCode code, string message)
          : this(code, message, null)
        {
        }

        internal WebSocketException(
          CloseStatusCode code, string message, Exception innerException
        )
          : base(message ?? code.GetMessage(), innerException)
        {
            _code = code;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the status code indicating the cause of the exception.
        /// </summary>
        /// <value>
        /// One of the <see cref="CloseStatusCode"/> enum values that represents
        /// the status code indicating the cause of the exception.
        /// </value>
        public CloseStatusCode Code
        {
            get
            {
                return _code;
            }
        }

        #endregion
    }
}
