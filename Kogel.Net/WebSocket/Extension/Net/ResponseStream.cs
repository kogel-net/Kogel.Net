using System;
using System.IO;
using System.Text;

namespace Kogel.Net.WebSocket.Extension.Net
{
    /// <summary>
    /// 
    /// </summary>
    internal class ResponseStream : Stream
    {
        private MemoryStream _bodyBuffer;
        private static readonly byte[] _crlf;
        private bool _disposed;
        private Stream _innerStream;
        private static readonly byte[] _lastChunk;
        private static readonly int _maxHeadersLength;
        private HttpListenerResponse _response;
        private bool _sendChunked;
        private Action<byte[], int, int> _write;
        private Action<byte[], int, int> _writeBody;
        private Action<byte[], int, int> _writeChunked;

        static ResponseStream()
        {
            _crlf = new byte[] { 13, 10 }; // "\r\n"
            _lastChunk = new byte[] { 48, 13, 10, 13, 10 }; // "0\r\n\r\n"
            _maxHeadersLength = 32768;
        }

        internal ResponseStream(Stream innerStream, HttpListenerResponse response, bool ignoreWriteExceptions)
        {
            _innerStream = innerStream;
            _response = response;

            if (ignoreWriteExceptions)
            {
                _write = writeWithoutThrowingException;
                _writeChunked = writeChunkedWithoutThrowingException;
            }
            else
            {
                _write = innerStream.Write;
                _writeChunked = writeChunked;
            }

            _bodyBuffer = new MemoryStream();
        }

        public override bool CanRead
        {
            get
            {
                return false;
            }
        }

        public override bool CanSeek
        {
            get
            {
                return false;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return !_disposed;
            }
        }

        public override long Length
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        public override long Position
        {
            get
            {
                throw new NotSupportedException();
            }

            set
            {
                throw new NotSupportedException();
            }
        }

        private bool flush(bool closing)
        {
            if (!_response.HeadersSent)
            {
                if (!flushHeaders())
                    return false;

                _response.HeadersSent = true;

                _sendChunked = _response.SendChunked;
                _writeBody = _sendChunked ? _writeChunked : _write;
            }

            flushBody(closing);

            return true;
        }

        private void flushBody(bool closing)
        {
            using (_bodyBuffer)
            {
                var len = _bodyBuffer.Length;

                if (len > Int32.MaxValue)
                {
                    _bodyBuffer.Position = 0;

                    var buffLen = 1024;
                    var buff = new byte[buffLen];
                    var nread = 0;

                    while (true)
                    {
                        nread = _bodyBuffer.Read(buff, 0, buffLen);

                        if (nread <= 0)
                            break;

                        _writeBody(buff, 0, nread);
                    }
                }
                else if (len > 0)
                {
                    _writeBody(_bodyBuffer.GetBuffer(), 0, (int)len);
                }
            }

            if (!closing)
            {
                _bodyBuffer = new MemoryStream();
                return;
            }

            if (_sendChunked)
                _write(_lastChunk, 0, 5);

            _bodyBuffer = null;
        }

        private bool flushHeaders()
        {
            if (!_response.SendChunked)
            {
                if (_response.ContentLength64 != _bodyBuffer.Length)
                    return false;
            }

            var statusLine = _response.StatusLine;
            var headers = _response.FullHeaders;

            var buff = new MemoryStream();
            var enc = Encoding.UTF8;

            using (var writer = new StreamWriter(buff, enc, 256))
            {
                writer.Write(statusLine);
                writer.Write(headers.ToStringMultiValue(true));
                writer.Flush();

                var start = enc.GetPreamble().Length;
                var len = buff.Length - start;

                if (len > _maxHeadersLength)
                    return false;

                _write(buff.GetBuffer(), start, (int)len);
            }

            _response.CloseConnection = headers["Connection"] == "close";

            return true;
        }

        private static byte[] getChunkSizeBytes(int size)
        {
            var chunkSize = String.Format("{0:x}\r\n", size);

            return Encoding.ASCII.GetBytes(chunkSize);
        }

        private void writeChunked(byte[] buffer, int offset, int count)
        {
            var size = getChunkSizeBytes(count);

            _innerStream.Write(size, 0, size.Length);
            _innerStream.Write(buffer, offset, count);
            _innerStream.Write(_crlf, 0, 2);
        }

        private void writeChunkedWithoutThrowingException(byte[] buffer, int offset, int count)
        {
            try
            {
                writeChunked(buffer, offset, count);
            }
            catch
            {
            }
        }

        private void writeWithoutThrowingException(byte[] buffer, int offset, int count)
        {
            try
            {
                _innerStream.Write(buffer, offset, count);
            }
            catch
            {
            }
        }

        internal void Close(bool force)
        {
            if (_disposed)
                return;

            _disposed = true;

            if (!force)
            {
                if (flush(true))
                {
                    _response.Close();

                    _response = null;
                    _innerStream = null;

                    return;
                }

                _response.CloseConnection = true;
            }

            if (_sendChunked)
                _write(_lastChunk, 0, 5);

            _bodyBuffer.Dispose();
            _response.Abort();

            _bodyBuffer = null;
            _response = null;
            _innerStream = null;
        }

        internal void InternalWrite(byte[] buffer, int offset, int count)
        {
            _write(buffer, offset, count);
        }

        public override IAsyncResult BeginRead(
          byte[] buffer,
          int offset,
          int count,
          AsyncCallback callback,
          object state
        )
        {
            throw new NotSupportedException();
        }

        public override IAsyncResult BeginWrite(
          byte[] buffer,
          int offset,
          int count,
          AsyncCallback callback,
          object state
        )
        {
            if (_disposed)
            {
                var name = GetType().ToString();

                throw new ObjectDisposedException(name);
            }

            return _bodyBuffer.BeginWrite(buffer, offset, count, callback, state);
        }

        public override void Close()
        {
            Close(false);
        }

        protected override void Dispose(bool disposing)
        {
            Close(!disposing);
        }

        public override int EndRead(IAsyncResult asyncResult)
        {
            throw new NotSupportedException();
        }

        public override void EndWrite(IAsyncResult asyncResult)
        {
            if (_disposed)
            {
                var name = GetType().ToString();

                throw new ObjectDisposedException(name);
            }

            _bodyBuffer.EndWrite(asyncResult);
        }

        public override void Flush()
        {
            if (_disposed)
                return;

            var sendChunked = _sendChunked || _response.SendChunked;

            if (!sendChunked)
                return;

            flush(false);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (_disposed)
            {
                var name = GetType().ToString();

                throw new ObjectDisposedException(name);
            }

            _bodyBuffer.Write(buffer, offset, count);
        }
    }
}
