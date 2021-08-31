using Kogel.Net.WebSocket.Enums;
using Kogel.Net.WebSocket.Extension.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kogel.Net.WebSocket.Extension
{
    public class MessageEventArgs : EventArgs
    {
        #region Private Fields

        private string _data;
        private bool _dataSet;
        private Opcode _opcode;
        private byte[] _rawData;

        #endregion

        #region Internal Constructors

        internal MessageEventArgs(WebSocketFrame frame)
        {
            _opcode = frame.Opcode;
            _rawData = frame.PayloadData.ApplicationData;
        }

        internal MessageEventArgs(Opcode opcode, byte[] rawData)
        {
            if ((ulong)rawData.LongLength > PayloadData.MaxLength)
                throw new WebSocketException(CloseStatusCode.TooBig);

            _opcode = opcode;
            _rawData = rawData;
        }

        #endregion

        #region Internal Properties

        /// <summary>
        /// Gets the opcode for the message.
        /// </summary>
        /// <value>
        /// <see cref="Opcode.Text"/>, <see cref="Opcode.Binary"/>,
        /// or <see cref="Opcode.Ping"/>.
        /// </value>
        internal Opcode Opcode
        {
            get
            {
                return _opcode;
            }
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the message data as a <see cref="string"/>.
        /// </summary>
        /// <value>
        /// A <see cref="string"/> that represents the message data if its type is
        /// text or ping and if decoding it to a string has successfully done;
        /// otherwise, <see langword="null"/>.
        /// </value>
        public string Data
        {
            get
            {
                setData();
                return _data;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the message type is binary.
        /// </summary>
        /// <value>
        /// <c>true</c> if the message type is binary; otherwise, <c>false</c>.
        /// </value>
        public bool IsBinary
        {
            get
            {
                return _opcode == Opcode.Binary;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the message type is ping.
        /// </summary>
        /// <value>
        /// <c>true</c> if the message type is ping; otherwise, <c>false</c>.
        /// </value>
        public bool IsPing
        {
            get
            {
                return _opcode == Opcode.Ping;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the message type is text.
        /// </summary>
        /// <value>
        /// <c>true</c> if the message type is text; otherwise, <c>false</c>.
        /// </value>
        public bool IsText
        {
            get
            {
                return _opcode == Opcode.Text;
            }
        }

        /// <summary>
        /// Gets the message data as an array of <see cref="byte"/>.
        /// </summary>
        /// <value>
        /// An array of <see cref="byte"/> that represents the message data.
        /// </value>
        public byte[] RawData
        {
            get
            {
                setData();
                return _rawData;
            }
        }

        #endregion

        #region Private Methods

        private void setData()
        {
            if (_dataSet)
                return;

            if (_opcode == Opcode.Binary)
            {
                _dataSet = true;
                return;
            }

            string data;
            if (_rawData.TryGetUTF8DecodedString(out data))
                _data = data;

            _dataSet = true;
        }

        #endregion
    }
}
