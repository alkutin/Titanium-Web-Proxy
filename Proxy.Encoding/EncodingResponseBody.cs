using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

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

        public byte[] PlainBody;

        public override Stream GetBody()
        {
            if (_bodyStream == null)
                _bodyStream = new MemoryStream(PlainBody);

            return _bodyStream;
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
