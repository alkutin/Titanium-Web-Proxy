using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
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
                removedItem.Dispose();            
        }

        public void ReceiveResponseBodyAsync(IEncodedAsyncResult requestAsyncResult, Action<EncodingResponseBody> onReceiveBody)
        {
            var folder = Path.Combine(Path.GetTempPath(), "ERemoteCache");
            var eTag = requestAsyncResult.ResponseHeaders.ETag;

            if (!string.IsNullOrEmpty(eTag))
            {
                var file = Path.Combine(folder, eTag);
                if (File.Exists(file))
                {                    
                    requestAsyncResult.ResponseBody = new EncodingResponseBody 
                    { 
                        Body = File.ReadAllBytes(file)
                    };

                    Task.Delay(5000).ContinueWith(t =>
                    {
                        EncodingAsyncResult removedItem;

                        if (_sessions.TryRemove(requestAsyncResult.Key, out removedItem))
                            removedItem.Dispose();
                    });

                    if (onReceiveBody != null)
                    {
                        onReceiveBody(requestAsyncResult.ResponseBody);
                    }

                    Debug.WriteLine(string.Concat("Returned from cache: ", eTag));
                    return;
                }
            }

            var httpRequest = CreateRequest("GET", requestAsyncResult.Key, "B=true&P=0&S=0");
            var responseTask = httpRequest.GetResponseAsync();
            responseTask.ContinueWith(task =>
            {
                var memoryStream = new MemoryStream();
                task.Result.GetResponseStream().CopyTo(memoryStream);
                var data = _encoder.Decode<EncodingResponseBody>(memoryStream.ToArray());
                requestAsyncResult.ResponseBody = data;

                var file = Path.Combine(folder, eTag);
                var fullFolder = Path.GetDirectoryName(file);
                if (!Directory.Exists(fullFolder))
                    Directory.CreateDirectory(fullFolder);

                File.WriteAllBytes(file, data.Body);

                Task.Delay(5000).ContinueWith(t =>
                {
                    EncodingAsyncResult removedItem;

                    if (_sessions.TryRemove(requestAsyncResult.Key, out removedItem))
                        removedItem.Dispose();
                });

                if (onReceiveBody != null)
                {
                    onReceiveBody(data);
                }
            });
        }

        public void ReceiveResponseHeaderAsync(IEncodedAsyncResult requestAsyncResult, Action<EncodingResponseHeader> onReceivedResponse)
        {
            var httpRequest = CreateRequest("GET", requestAsyncResult.Key, "B=false&P=0&S=0");
            var responseTask = httpRequest.GetResponseAsync();
            responseTask.ContinueWith(task =>
            {
                var memoryStream = new MemoryStream();
                task.Result.GetResponseStream().CopyTo(memoryStream);
                var data = _encoder.Decode<EncodingResponseHeader>(memoryStream.ToArray());
                requestAsyncResult.ResponseHeaders = data;
                if (onReceivedResponse != null)
                {
                    onReceivedResponse(data);
                }
            });
        }

        public IEncodedAsyncResult SendRequestAsync(KeyValuePair<EncodingRequestHeader, EncodingRequestBody> request, Action<IEncodedAsyncResult> onComplete)
        {
            var httpRequest = WebRequest.CreateHttp(new Uri(Url));
            httpRequest.Method = "POST";
            httpRequest.ContentType = "application/content-stream";
            httpRequest.Proxy = null;
            var encodingRequest = _encoder.Encode(new EncodingRequest { Header = request.Key, Body = request.Value });
            httpRequest.GetRequestStream().Write(encodingRequest, 0, encodingRequest.Length);

            var response = httpRequest.GetResponse();
            var memoryStream = new MemoryStream();
            response.GetResponseStream().CopyTo(memoryStream);
            var key = _encoder.Decode<Guid>(memoryStream.ToArray());
            var asyncResult = new EncodingAsyncResult { Key = key, RequestHeaders = request.Key };
            _sessions.TryAdd(key, asyncResult);

            if (onComplete != null)            
                Task.Run(() => { onComplete(asyncResult); });
            
            return asyncResult;
        }

        HttpWebRequest CreateRequest(string httpMethod, Guid key, string additionalParams)
        {
            var request = WebRequest.CreateHttp(new Uri(string.Format("{0}?Key={1}&{2}", Url, key, additionalParams)));
            request.Method = httpMethod;
            request.ContentType = "application/content-stream";
            request.Proxy = null;
            return request;
        }
    }
}
