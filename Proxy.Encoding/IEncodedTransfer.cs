using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Proxy.Encoding
{
    public interface IEncodedTransfer
    {
        IEncodedAsyncResult SendRequestAsync(KeyValuePair<EncodingRequestHeader, EncodingRequestBody> request,
            Action<IEncodedAsyncResult> onComplete);
        void ReceiveResponseHeaderAsync(IEncodedAsyncResult requestAsyncResult, Action<EncodingResponseHeader> onReceivedResponse);
        void ReceiveResponseBodyAsync(IEncodedAsyncResult requestAsyncResult, Action<EncodingResponseBody> onReceiveBody);
        void Abort(IEncodedAsyncResult requestAsyncResult);
    }
}