using Kogel.Net.WebSocket.Extension.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kogel.Net.WebSocket.Extension
{
    /// <summary>
    /// 关闭事件
    /// </summary>
    public class CloseEventArgs : EventArgs
    {
        private bool _clean;
        private PayloadData _payloadData;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="payloadData"></param>
        /// <param name="clean"></param>
        internal CloseEventArgs(PayloadData payloadData, bool clean)
        {
            _payloadData = payloadData;
            _clean = clean;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="code"></param>
        /// <param name="reason"></param>
        /// <param name="clean"></param>
        internal CloseEventArgs(ushort code, string reason, bool clean)
        {
            _payloadData = new PayloadData(code, reason);
            _clean = clean;
        }

        /// <summary>
        /// 获取连接关闭的状态代码
        /// </summary>
        public ushort Code
        {
            get
            {
                return _payloadData.Code;
            }
        }

        /// <summary>
        /// 获取连接关闭的原因
        /// </summary>
        public string Reason
        {
            get
            {
                return _payloadData.Reason;
            }
        }

        /// <summary>
        /// 获取一个值，该值指示连接是否已完全关闭
        /// </summary>
        public bool WasClean
        {
            get
            {
                return _clean;
            }
        }
    }
}
