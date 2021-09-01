using System;

namespace Kogel.Net.WebSocket.Enums
{
    internal enum InputChunkState
    {
        None,
        Data,
        DataEnded,
        Trailer,
        End
    }
}
