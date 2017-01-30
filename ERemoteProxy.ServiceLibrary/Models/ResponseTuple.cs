using System;
using Proxy.Encoding;

namespace ERemoteProxy.ServiceLibrary.Models
{
    public class ResponseTuple
    {
        public IEncodedAsyncResult AsyncResult;
        public EncodingRequestHeader RequestHeader;
        public EncodingRequestBody RequestBody;
        public EncodingResponseHeader ResponseHeader;
        public TwoWayEncodingResponseBody ResponseBody;
        public DateTime LastAccessUTC;
    }
}
