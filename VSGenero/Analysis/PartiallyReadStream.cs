using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis
{
    /// <summary>
    /// Creates a stream out of a some bytes we've read and the stream we read them from.  This allows us to
    /// not require a seekable stream for our parser.
    /// </summary>
    class PartiallyReadStream : Stream
    {
        private readonly List<byte> _readBytes;
        private readonly Stream _stream;
        private long _position;

        public PartiallyReadStream(List<byte> readBytes, Stream stream)
        {
            _readBytes = readBytes;
            _stream = stream;
        }

        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override void Flush()
        {
            throw new InvalidOperationException();
        }

        public override long Length
        {
            get { return _stream.Length; }
        }

        public override long Position
        {
            get
            {
                if (_position == -1)
                {
                    return _stream.Position;
                }
                return _position;
            }
            set
            {
                throw new InvalidOperationException();
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (_position == -1)
            {
                return _stream.Read(buffer, offset, count);
            }
            else
            {
                int bytesRead = 0;
                for (int i = 0; i < count && _position < _readBytes.Count; i++)
                {
                    buffer[i + offset] = _readBytes[(int)_position];
                    _position++;
                    bytesRead++;
                }

                if (_position == _readBytes.Count)
                {
                    _position = -1;
                    if (bytesRead != count)
                    {
                        var res = _stream.Read(buffer, offset + bytesRead, count - bytesRead);
                        if (res == -1)
                        {
                            return bytesRead;
                        }
                        return res + bytesRead;
                    }
                }
                return bytesRead;
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new InvalidOperationException();
        }

        public override void SetLength(long value)
        {
            throw new InvalidOperationException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new InvalidOperationException();
        }
    }
}
