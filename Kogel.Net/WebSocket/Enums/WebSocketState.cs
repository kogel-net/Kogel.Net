using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kogel.Net.WebSocket.Enums
{
    /// <summary>
    /// websocket连接的状态
    /// </summary>
    public enum WebSocketState
    {
        /// <summary>
        /// 连接中
        /// </summary>
        Connecting = 0,

        /// <summary>
        /// 打开(已连接)
        /// </summary>
        Open = 1,

        /// <summary>
        /// 关闭中
        /// </summary>
        Closing = 2,

        /// <summary>
        /// 已关闭
        /// </summary>
        Closed = 3
    }
}
