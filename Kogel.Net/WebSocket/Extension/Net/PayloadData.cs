using Kogel.Net.WebSocket.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kogel.Net.WebSocket.Extension.Net
{
    internal class PayloadData : IEnumerable<byte>
    {
        private byte[] _data;
        private long _extDataLength;
        private long _length;

        /// <summary>
        /// 
        /// </summary>
        public static readonly PayloadData Empty;

        /// <summary>
        /// 
        /// </summary>
        public static readonly ulong MaxLength;

        static PayloadData()
        {
            Empty = new PayloadData(WebSocket.EmptyBytes, 0);
            MaxLength = Int64.MaxValue;
        }
        internal PayloadData(byte[] data) : this(data, data.LongLength)
        {
        }

        internal PayloadData(byte[] data, long length)
        {
            _data = data;
            _length = length;
        }

        internal PayloadData(ushort code, string reason)
        {
            _data = code.Append(reason);
            _length = _data.LongLength;
        }

        internal ushort Code
        {
            get
            {
                return _length >= 2
                       ? _data.SubArray(0, 2).ToUInt16(ByteOrder.Big)
                       : (ushort)1005;
            }
        }

        internal long ExtensionDataLength
        {
            get
            {
                return _extDataLength;
            }

            set
            {
                _extDataLength = value;
            }
        }

        internal bool HasReservedCode
        {
            get
            {
                return _length >= 2 && Code.IsReserved();
            }
        }

        internal string Reason
        {
            get
            {
                if (_length <= 2)
                    return String.Empty;

                var raw = _data.SubArray(2, _length - 2);

                string reason;
                return raw.TryGetUTF8DecodedString(out reason)
                       ? reason
                       : String.Empty;
            }
        }

        public byte[] ApplicationData
        {
            get
            {
                return _extDataLength > 0
                       ? _data.SubArray(_extDataLength, _length - _extDataLength)
                       : _data;
            }
        }

        public byte[] ExtensionData
        {
            get
            {
                return _extDataLength > 0
                       ? _data.SubArray(0, _extDataLength)
                       : WebSocket.EmptyBytes;
            }
        }

        public ulong Length
        {
            get
            {
                return (ulong)_length;
            }
        }

        internal void Mask(byte[] key)
        {
            for (long i = 0; i < _length; i++)
                _data[i] = (byte)(_data[i] ^ key[i % 4]);
        }

        public IEnumerator<byte> GetEnumerator()
        {
            foreach (var b in _data)
                yield return b;
        }

        public byte[] ToArray()
        {
            return _data;
        }

        public override string ToString()
        {
            return BitConverter.ToString(_data);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
