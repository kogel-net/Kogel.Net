using Kogel.Net.WebSocket.Extension.Net;
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
    public class CloseEventArgs : EventArgs
    {
        #region Private Fields

        private bool _clean;
        private PayloadData _payloadData;

        #endregion

        #region Internal Constructors

        internal CloseEventArgs(PayloadData payloadData, bool clean)
        {
            _payloadData = payloadData;
            _clean = clean;
        }

        internal CloseEventArgs(ushort code, string reason, bool clean)
        {
            _payloadData = new PayloadData(code, reason);
            _clean = clean;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the status code for the connection close.
        /// </summary>
        /// <value>
        /// A <see cref="ushort"/> that represents the status code for
        /// the connection close if present.
        /// </value>
        public ushort Code
        {
            get
            {
                return _payloadData.Code;
            }
        }

        /// <summary>
        /// Gets the reason for the connection close.
        /// </summary>
        /// <value>
        /// A <see cref="string"/> that represents the reason for
        /// the connection close if present.
        /// </value>
        public string Reason
        {
            get
            {
                return _payloadData.Reason;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the connection has been closed cleanly.
        /// </summary>
        /// <value>
        /// <c>true</c> if the connection has been closed cleanly; otherwise,
        /// <c>false</c>.
        /// </value>
        public bool WasClean
        {
            get
            {
                return _clean;
            }
        }

        #endregion
    }
}
