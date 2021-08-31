using Kogel.Net.WebSocket.Enums;
using Kogel.Net.WebSocket.Extension.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kogel.Net.WebSocket.Extension
{
    /// <summary>
    /// 消息事件
    /// </summary>
    public class MessageEventArgs : EventArgs
    {
        private string _data;
        private bool _dataSet;
        private Opcode _opcode;
        private byte[] _rawData;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="frame"></param>
        internal MessageEventArgs(WebSocketFrame frame)
        {
            _opcode = frame.Opcode;
            _rawData = frame.PayloadData.ApplicationData;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="opcode"></param>
        /// <param name="rawData"></param>
        internal MessageEventArgs(Opcode opcode, byte[] rawData)
        {
            if ((ulong)rawData.LongLength > PayloadData.MaxLength)
                throw new WebSocketException(CloseStatusCode.TooBig);

            _opcode = opcode;
            _rawData = rawData;
        }

        /// <summary>
        /// 获取消息的操作码
        /// </summary>
        internal Opcode Opcode
        {
            get
            {
                return _opcode;
            }
        }



        /// <summary>
        /// 获取消息数据作为字符串
        /// </summary>
        public string Data
        {
            get
            {
                SetData();
                return _data;
            }
        }

        /// <summary>
        /// 获取指示消息类型是否为二进制的值
        /// </summary>
        public bool IsBinary
        {
            get
            {
                return _opcode == Opcode.Binary;
            }
        }

        /// <summary>
        /// 获取指示消息类型是否为 ping 的值
        /// </summary>
        public bool IsPing
        {
            get
            {
                return _opcode == Opcode.Ping;
            }
        }

        /// <summary>
        /// 获取指示消息类型是否为文本
        /// </summary>
        public bool IsText
        {
            get
            {
                return _opcode == Opcode.Text;
            }
        }

        /// <summary>
        /// 以数组的形式获取消息数据
        /// </summary>
        public byte[] RawData
        {
            get
            {
                SetData();
                return _rawData;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void SetData()
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
    }
}
