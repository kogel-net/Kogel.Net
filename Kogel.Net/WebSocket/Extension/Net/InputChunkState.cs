using System;

namespace Kogel.Net.WebSocket.Extension.Net
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
