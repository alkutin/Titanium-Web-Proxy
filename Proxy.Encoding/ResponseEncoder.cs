using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EndPointProxy;
using ProxyLanguage;

namespace Proxy.Encoding
{
    public class ResponseEncoder : IEncodedTransfer
    {
        IProxyRequest _proxyRequest;
        private EncodingRequestHeader _requestHeaders;
        private EncodingRequestBody _requestBody;
        private Task _requestBodyTask;
        public ResponseEncoder()
        {            
        }

        public IEncodedAsyncResult SendRequestAsync(KeyValuePair<EncodingRequestHeader, EncodingRequestBody> request, Action<IEncodedAsyncResult> onComplete)
        {
            _requestHeaders = request.Key;
            _requestBody = request.Value;
            _proxyRequest = new EndPointProxyRequest(_requestHeaders.RequestUri, _requestHeaders.HttpMethod, _requestHeaders.Version);
            _proxyRequest.SetRequestHeaders(_requestHeaders.RequestHeaders);

            if (_requestBody.Body != null && _requestBody.Body.Length > 0)
            {
                _requestBodyTask = new MemoryStream(_requestBody.Body).CopyToAsync(_proxyRequest.GetRequestStream());
            }

            //return new 
            throw new NotImplementedException();
        }

        public void ReceiveResponseHeaderAsync(IEncodedAsyncResult requestAsyncResult, Action<EncodingResponseHeader> onReceivedResponse)
        {
            throw new NotImplementedException();
        }

        public void ReceiveResponseBodyAsync(IEncodedAsyncResult requestAsyncResult, Action<EncodingResponseBody> onReceiveBody)
        {
            throw new NotImplementedException();
        }

        public void Abort(IEncodedAsyncResult requestAsyncResult)
        {
            throw new NotImplementedException();
        }
    }
}
