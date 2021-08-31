using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kogel.Net.WebSocket.Enums
{
    /// <summary>
    /// 服务状态
    /// </summary>
    public enum ServerState
    {
        Ready,
        Start,
        ShuttingDown,
        Stop
    }
}
