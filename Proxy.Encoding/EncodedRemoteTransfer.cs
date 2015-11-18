using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Proxy.Encoding
{
    public class EncodedRemoteTransfer : IEncodedTransfer
    {
        static ConcurrentDictionary<Guid, EncodingAsyncResult> _sessions = new ConcurrentDictionary<Guid, EncodingAsyncResult>();
        private Encoder _encoder;

        public string Url { get { return ConfigurationManager.AppSettings["ApiUrl"]; } }

        public byte[] Key { get { return System.Text.Encoding.ASCII.GetBytes(ConfigurationManager.AppSettings["Key"]); } }
        public byte[] Vector { get { return System.Text.Encoding.ASCII.GetBytes(ConfigurationManager.AppSettings["Vector"]); } }
        
        public EncodedRemoteTransfer()
        {
            _encoder = new Encoder(Key, Vector);
        }

        public void Abort(IEncodedAsyncResult requestAsyncResult)
        {
            var request = CreateRequest("DELETE", requestAsyncResult.Key, string.Empty);            
            request.GetResponseAsync();
            EncodingAsyncResult removedItem;
            if (_sessions.TryRemove(requestAsyncResult.Key, out removedItem))
            {
                removedItem.Dispose();
            }
        }

        public void ReceiveResponseBodyAsync(IEncodedAsyncResult requestAsyncResult, Action<EncodingResponseBody> onReceiveBody)
        {
            throw new NotImplementedException();
        }

        public void ReceiveResponseHeaderAsync(IEncodedAsyncResult requestAsyncResult, Action<EncodingResponseHeader> onReceivedResponse)
        {
            var httpRequest = CreateRequest("GET", requestAsyncResult.Key, "body=false");
            //var response = await httpRequest.GetResponseAsync();
            throw new NotImplementedException();
        }

        public IEncodedAsyncResult SendRequestAsync(KeyValuePair<EncodingRequestHeader, EncodingRequestBody> request, Action<IEncodedAsyncResult> onComplete)
        {
            var httpRequest = WebRequest.CreateHttp(new Uri(Url));
            httpRequest.Method = "POST";
            var encodingRequest = _encoder.Encode(new EncodingRequest { Header = request.Key, Body = request.Value });
            httpRequest.GetRequestStream().Write(encodingRequest, 0, encodingRequest.Length);

            var response = httpRequest.GetResponse();
            var memoryStream = new MemoryStream();
            response.GetResponseStream().CopyTo(memoryStream);
            var key = _encoder.Decode<Guid>(memoryStream.ToArray());
            var asyncResult = new EncodingAsyncResult { Key = key };
            _sessions.TryAdd(key, asyncResult);

            if (onComplete != null)            
                Task.Run(() => { onComplete(asyncResult); });
            
            return asyncResult;
        }

        HttpWebRequest CreateRequest(string httpMethod, Guid key, string additionalParams)
        {
            var request = WebRequest.CreateHttp(new Uri(string.Format("{0}?Key={1}&{2}", Url, key, additionalParams)));
            request.Method = httpMethod;
            return request;
        }
    }
}
