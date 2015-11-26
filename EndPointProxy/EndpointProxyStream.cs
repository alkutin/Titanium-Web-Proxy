using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EndPointProxy
{
    public class EndpointProxyStream : Stream, IDisposable
    {
        private Stream _innerStream;

        public EndpointProxyStream(Stream innerStream)
        {
            _innerStream = innerStream;
        }

        public override bool CanRead
        {
            get
            {
                return _innerStream.CanRead;
            }
        }

        public override bool CanSeek
        {
            get
            {
                return _innerStream.CanSeek;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return _innerStream.CanWrite;
            }
        }

        public override long Length
        {
            get
            {
                return _innerStream.Length;
            }
        }

        public override long Position
        {
            get
            {
                return _innerStream.Position;
            }

            set
            {
                _innerStream.Position = value;
            }
        }

        public override void Flush()
        {
            if (_innerStream != null)
                _innerStream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            try
            {
                return _innerStream.Read(buffer, offset, count);
            }
            catch (IOException ioError)
            {
                Debug.WriteLine(ioError.ToString());
                return 0;
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return _innerStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            _innerStream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _innerStream.Write(buffer, offset, count);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _innerStream != null)
            {
                _innerStream.Dispose();                
            }
                
            base.Dispose(disposing);
        }
    }
}
