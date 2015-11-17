using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proxy.Encoding
{
    public class EncodedRemoteTransfer : IEncodedTransfer
    {
        public void Abort(IEncodedAsyncResult requestAsyncResult)
        {
            throw new NotImplementedException();
        }

        public void ReceiveResponseBodyAsync(IEncodedAsyncResult requestAsyncResult, Action<EncodingResponseBody> onReceiveBody)
        {
            throw new NotImplementedException();
        }

        public void ReceiveResponseHeaderAsync(IEncodedAsyncResult requestAsyncResult, Action<EncodingResponseHeader> onReceivedResponse)
        {
            throw new NotImplementedException();
        }

        public IEncodedAsyncResult SendRequestAsync(KeyValuePair<EncodingRequestHeader, EncodingRequestBody> request, Action<IEncodedAsyncResult> onComplete)
        {
            throw new NotImplementedException();
        }
    }
}
