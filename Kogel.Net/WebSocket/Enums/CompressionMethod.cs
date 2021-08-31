using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kogel.Net.WebSocket.Enums
{
    /// <summary>
    /// 压缩类型
    /// </summary>
    public enum CompressionMethod : byte
    {
        /// <summary>
        /// Specifies no compression.
        /// </summary>
        None,
        /// <summary>
        /// Specifies DEFLATE.
        /// </summary>
        Deflate
    }
}
