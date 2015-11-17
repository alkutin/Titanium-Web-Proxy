using Proxy.Encoding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RemoteProxySite.Handlers
{
    /// <summary>
    /// Summary description for ERemote
    /// </summary>
    public class ERemote : IHttpHandler, IEncodedTransfer
    {

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/plain";
            context.Response.Write("Hello World");
        }

        public IEncodedAsyncResult SendRequestAsync(KeyValuePair<EncodingRequestHeader, EncodingRequestBody> request, Action<IEncodedAsyncResult> onComplete)
        {
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

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}