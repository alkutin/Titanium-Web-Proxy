using Proxy.Encoding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteProxySite.Models
{
    class ResponseTuple
    {
        public IEncodedAsyncResult AsyncResult;
        public EncodingRequestHeader RequestHeader;
        public EncodingRequestBody RequestBody;
        public EncodingResponseHeader ResponseHeader;
        public TwoWayEncodingResponseBody ResponseBody;
    }
}
