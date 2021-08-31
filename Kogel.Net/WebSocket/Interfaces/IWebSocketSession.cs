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
    public interface IWebSocketSession
    {
        #region Properties

        /// <summary>
        /// Gets the current state of the WebSocket connection for the session.
        /// </summary>
        /// <value>
        ///   <para>
        ///   One of the <see cref="WebSocketState"/> enum values.
        ///   </para>
        ///   <para>
        ///   It indicates the current state of the connection.
        ///   </para>
        /// </value>
        WebSocketState ConnectionState { get; }

        /// <summary>
        /// Gets the information in the WebSocket handshake request.
        /// </summary>
        /// <value>
        /// A <see cref="WebSocketContext"/> instance that provides the access to
        /// the information in the handshake request.
        /// </value>
        WebSocketContext Context { get; }

        /// <summary>
        /// Gets the unique ID of the session.
        /// </summary>
        /// <value>
        /// A <see cref="string"/> that represents the unique ID of the session.
        /// </value>
        string ID { get; }

        /// <summary>
        /// Gets the name of the WebSocket subprotocol for the session.
        /// </summary>
        /// <value>
        /// A <see cref="string"/> that represents the name of the subprotocol
        /// if present.
        /// </value>
        string Protocol { get; }

        /// <summary>
        /// Gets the time that the session has started.
        /// </summary>
        /// <value>
        /// A <see cref="DateTime"/> that represents the time that the session
        /// has started.
        /// </value>
        DateTime StartTime { get; }

        #endregion
    }
}
