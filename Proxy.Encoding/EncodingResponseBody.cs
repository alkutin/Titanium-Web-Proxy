using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace Proxy.Encoding
{
    public abstract class EncodingResponseBody
    {
        protected EncodingResponseBody() { }

        public abstract Stream GetBody();
    }

    public class PlainEncodingResponseBody : EncodingResponseBody
    {
        private MemoryStream _bodyStream;

        public PlainEncodingResponseBody() : base() { }

        public int Position;
        public byte[] PlainBody;

        public override Stream GetBody()
        {
            if (_bodyStream == null)
                _bodyStream = new MemoryStream(PlainBody);

            return _bodyStream;
        }
    }

    public class TwoWayEncodingResponseBody : EncodingResponseBody
    {
        private MemoryStream _bodyStream;
        
        public TwoWayEncodingResponseBody() : base() { }

        public int Position;
        public bool WriteDone { get; set; }
        
        public Stream BodyStream { get; set; }

        public override Stream GetBody()
        {
            return BodyStream;
        }

        public PlainEncodingResponseBody CreatePlain()
        {
            var memStream = new MemoryStream(); 
            
            WaitForWriteDone();
            BodyStream.CopyTo(memStream);
            
            var rez = new PlainEncodingResponseBody
            {
                Position = Position,
                PlainBody = memStream.ToArray()
            };
            return rez;
        }

        private void WaitForWriteDone()
        {
            while (!WriteDone)
            {
                Thread.Sleep(50);
            }
        }
    }

    public class StreamEncodingResponseBody : EncodingResponseBody
    {
        public StreamEncodingResponseBody() : base() { }

        public Stream BodyStream { get; set; }

        public override Stream GetBody()
        {
            return BodyStream;
        }
    }
}
