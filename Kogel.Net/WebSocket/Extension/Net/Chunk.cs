using System;

namespace Kogel.Net.WebSocket.Extension.Net
{
    internal class Chunk
    {
        private byte[] _data;
        private int _offset;

        public Chunk(byte[] data)
        {
            _data = data;
        }

        public int ReadLeft
        {
            get
            {
                return _data.Length - _offset;
            }
        }

        public int Read(byte[] buffer, int offset, int count)
        {
            var left = _data.Length - _offset;

            if (left == 0)
                return 0;

            if (count > left)
                count = left;

            Buffer.BlockCopy(_data, _offset, buffer, offset, count);

            _offset += count;

            return count;
        }
    }
}
