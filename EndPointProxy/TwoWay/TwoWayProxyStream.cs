using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EndPointProxy.TwoWay
{
    public class TwoWayProxyStream : Stream
    {
        private TwoWayStoreStream _storeStream;
        private long _position;

        public TwoWayProxyStream(TwoWayStoreStream storeStream)
        {
            _storeStream = storeStream;
        }

        public override bool CanRead
        {
            get { return _storeStream.CanRead; }
        }

        public override bool CanSeek
        {
            get { return _storeStream.CanRead; }
        }

        public override bool CanWrite
        {
            get { return _storeStream.CanWrite; }
        }

        public override void Flush()
        {
            _storeStream.Flush();
        }

        public override long Length
        {
            get { return _storeStream.Length; }
        }

        public override long Position
        {
            get
            {
                lock(_storeStream.Lock)
                {
                    return _position;
                }
            }
            set
            {
                lock (_storeStream.Lock)
                {
                    _position = value;
                }
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            lock (_storeStream.Lock)
            {
                var movePosition = _storeStream.Store.Seek(_position, SeekOrigin.Begin);
                if (movePosition != _position)
                    throw new IOException("Bad read position");

                var rez = _storeStream.Store.Read(buffer, offset, count);
                _position += rez;
                return rez;
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            lock (_storeStream.Lock)
            {
                switch (origin)
                {
                    case SeekOrigin.Begin:
                        _position = offset;
                        break;
                    case SeekOrigin.Current:
                        _position += offset;
                        break;
                    case SeekOrigin.End:
                        _position = _storeStream.Store.Length + offset;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("origin");
                }

                return _position;
            }
        }

        public override void SetLength(long value)
        {
            _storeStream.SetLength(value);            
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            lock (_storeStream.Lock)
            {
                var movePosition = _storeStream.Store.Seek(_position, SeekOrigin.Begin);
                if (movePosition != _position)
                    throw new IOException("Bad write position");

                _storeStream.Store.Write(buffer, offset, count);
                _position += count;
            }
        }
    }
}
