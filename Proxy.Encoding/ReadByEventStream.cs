using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proxy.Encoding
{
    public class ReadByEventStream : Stream
    {
        private long _position;
        private long _streamSize;
        private Func<long, long, byte[]> _onRead;
        
        public ReadByEventStream(long streamSize, Func<long, long, byte[]> onRead)
        {
            _streamSize = streamSize;
            _onRead = onRead;
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
            
        }

        public override long Length
        {
            get { return _streamSize; }
        }

        public override long Position
        {
            get
            {
                return _position;
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var data = _onRead(Position, count);            
            var blockSize = Math.Min(data.Length, buffer.Length - offset);
            _position += blockSize;
            Array.Copy(data, 0, buffer, offset, blockSize);
            return blockSize;
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
            throw new NotSupportedException();
        }
    }
}
