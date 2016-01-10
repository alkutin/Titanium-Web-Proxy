using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EndPointProxy.TwoWay
{
    public class TwoWayStoreStream : Stream
    {
        private object _lock = new object();
        private MemoryStream _store = new MemoryStream();

        public object Lock { get { return _lock; } }
        public MemoryStream Store { get { return _store; } }

        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanSeek
        {
            get { return true; }
        }

        public override bool CanWrite
        {
            get { return true; }
        }

        public override void Flush()
        {
            lock (_lock)
            {
                _store.Flush();
            }
        }

        public override long Length
        {
            get
            {
                lock (_lock)
                {
                    return _store.Length;
                }
            }
        }

        public override long Position
        {
            get
            {
                lock (_lock)
                {
                    return _store.Position;
                }
            }
            set
            {
                lock (_lock)
                {
                    _store.Position = value;
                }
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            lock (_lock)
            {
                return _store.Read(buffer, offset, count);
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            lock (_lock)
            {
                return _store.Seek(offset, origin);
            }
        }

        public override void SetLength(long value)
        {
            lock (_lock)
            {
                _store.SetLength(value);
            }
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            lock (_lock)
            {
                _store.Write(buffer, offset, count);
            }
        }
    }
}
