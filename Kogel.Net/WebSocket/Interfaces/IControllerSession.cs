using Kogel.Net.WebSocket.Enums;
using Kogel.Net.WebSocket.Extension.Net.WebSockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kogel.Net.WebSocket.Interfaces
{
    /// <summary>
    /// 在 WebSocket 会话中公开对信息的访问
    /// </summary>
    public interface IControllerSession
    {
        /// <summary>
        /// 获取会话的 WebSocket 连接的当前状态。
        /// </summary>
        WebSocketState ConnectionState { get; }

        /// <summary>
        /// 获取服务的 WebSocket 握手请求中的信息。
        /// </summary>
        WebSocketContext Context { get; }

        /// <summary>
        /// 获取会话的唯一ID
        /// </summary>
        string Id { get; }

        /// <summary>
        /// 获取或设置服务的 WebSocket 子协议的名称。
        /// </summary>
        string Protocol { get; }

        /// <summary>
        /// 获取会话开始的时间
        /// </summary>
        DateTime StartTime { get; }
    }
}
