using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EndPointProxy;
using ProxyLanguage;
using ProxyLanguage.Models;

namespace Proxy.Encoding
{
    public class ResponseEncoder : IEncodedTransfer
    {
        IProxyRequest _proxyRequest;
        private EncodingRequestHeader _requestHeaders;
        private EncodingRequestBody _requestBody;
        private Task _requestBodyTask;
        private IAsyncResult _headersResponseAsync;
        private IProxyResponse _proxyResponse;
        private EncodingAsyncResult _encodingAsyncResult;

        public ResponseEncoder()
        {            
        }

        public IEncodedAsyncResult SendRequestAsync(KeyValuePair<EncodingRequestHeader, EncodingRequestBody> request, Action<IEncodedAsyncResult> onComplete)
        {
            _requestHeaders = request.Key;
            _requestBody = request.Value;
            _proxyRequest = new EndPointProxyRequest(_requestHeaders.RequestUri, _requestHeaders.HttpMethod, Version.Parse(_requestHeaders.Version));
            _proxyRequest.SetRequestHeaders(_requestHeaders.RequestHeaders);
            
            _encodingAsyncResult = new EncodingAsyncResult()
            {
                Key = Guid.NewGuid(),
                RequestHeaders = _requestHeaders
            };


            if (_requestBody.Body != null && _requestBody.Body.Length > 0)
                _requestBodyTask = new MemoryStream(_requestBody.Body).CopyToAsync(_proxyRequest.GetRequestStream());
            else
                _requestBodyTask = Task.Delay(0);

            _requestBodyTask.ContinueWith((task) => {
                _headersResponseAsync = _proxyRequest.BeginGetResponse((response) =>
                {
                    if (onComplete != null)
                        Task.Run(() => { onComplete(_encodingAsyncResult); });

                    Task.Run(() =>
                    {
                        _proxyResponse = _proxyRequest.EndGetResponse(response);

                        long contentLength;
                        if (!long.TryParse(_proxyResponse.GetResponseHeader("Content-Length"), out contentLength))
                            contentLength = int.MaxValue;

                        _encodingAsyncResult.ResponseHeaders = new EncodingResponseHeader
                        {
                            HttpCode = _proxyResponse.StatusCode,
                            HttpDescription = _proxyResponse.StatusDescription,
                            ContentEncoding = _proxyResponse.ContentEncoding,
                            HasBody = contentLength != 0 || _proxyResponse.StatusCode != System.Net.HttpStatusCode.Created,
                            ResponseHeaders = _proxyResponse.Headers.Select(s => new HttpHeader(s.Key, s.Value)).ToList(),
                            ETag = _proxyResponse.GetResponseHeader("ETag")
                        };

                        var memStream = new MemoryStream();
                        _proxyResponse.GetResponseStream().CopyToAsync(memStream).ContinueWith(w =>
                        {
                            _encodingAsyncResult.ResponseBody = new PlainEncodingResponseBody { PlainBody = memStream.ToArray() };

                        });
                    });
                    
                }, this);
            });
            

            
            return _encodingAsyncResult;
        }

        public void ReceiveResponseHeaderAsync(IEncodedAsyncResult requestAsyncResult, Action<EncodingResponseHeader> onReceivedResponse)
        {            
            //_requestBodyTask.Wait();
            if (_encodingAsyncResult != null)
                _encodingAsyncResult.WaitForHeader();
                                        
            if (onReceivedResponse != null)
            {
                onReceivedResponse(requestAsyncResult.ResponseHeaders);
            }
        }

        public void ReceiveResponseBodyAsync(IEncodedAsyncResult requestAsyncResult, Action<EncodingResponseBody> onReceiveBody)
        {
            if (_encodingAsyncResult != null)
                _encodingAsyncResult.WaitForBody();

            if (onReceiveBody != null)
            {
                onReceiveBody(requestAsyncResult.ResponseBody);
            }
        }

        public void Abort(IEncodedAsyncResult requestAsyncResult)
        {
            if (_requestBodyTask != null)
                _requestBodyTask.Dispose();
            if (_proxyResponse != null)
                _proxyResponse.Close();
        }
    }
}
