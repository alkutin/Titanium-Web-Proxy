using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProxyLanguage;
using ProxyLanguage.Models;

namespace Proxy.Encoding
{
    public class RequestEncoder : IProxyRequest
    {
        private IEncodedTransfer _encodedTransfer;        
        private List<ProxyLanguage.Models.HttpHeader> _requestHeaders;
        private Uri _requestUri;
        private string _httpMethod;
        private Version _version;

        private bool _allowWriteStreamBuffering = true;
        private MemoryStream _requestStream = new MemoryStream();
        private TimeSpan WaitTimeoutMSecs = TimeSpan.FromMinutes(10);
        private IEncodedAsyncResult _asyncResult;

        public RequestEncoder(IEncodedTransfer encodedTransfer, Uri requestUri, string httpMethod, Version version)
        {
            _encodedTransfer = encodedTransfer;
            _requestUri = requestUri;
            _httpMethod = httpMethod;
            _version = version;            
        }

        public Uri RequestUri
        {
            get { return _requestUri; }
        }

        public bool KeepAlive
        {
            get
            {
                bool bRez;
                return bool.TryParse(GetRequestHeader("Keep-Alive"), out bRez) ? bRez : false;
            }
        }

        public System.Text.Encoding RequestEncoding
        {
            get 
            {
                var contentType = GetRequestHeader("Content-Type");
                if (!string.IsNullOrEmpty(contentType))
                {
                    var encodingSplit = contentType.Split('=');
                    if (encodingSplit.Length == 2 && encodingSplit[0].ToLower().Trim() == "charset")
                    {
                        return System.Text.Encoding.GetEncoding(encodingSplit[1]);
                    }
                }

                return System.Text.Encoding.GetEncoding("ISO-8859-1");
            }
        }

        public long ContentLength
        {
            get
            {
                long iRez;
                return long.TryParse(GetRequestHeader("Content-Length"), out iRez) ? iRez : 0;
            }
            set
            {
                SetRequestHeader("Content-Length", value.ToString());
            }
        }

        public string Method
        {
            get { return _httpMethod; }
        }

        public bool AllowWriteStreamBuffering
        {
            get
            {
                return _allowWriteStreamBuffering;
            }
            set
            {
                _allowWriteStreamBuffering = value;
            }
        }

        public bool SendChunked
        {
            get 
            {
                return GetRequestHeader("Transfer-Encoding").ToLower().Contains("chunked");
            }
        }

        public void SetRequestHeaders(List<ProxyLanguage.Models.HttpHeader> requestHeaders)
        {
            _requestHeaders = requestHeaders.ToList();
        }

        public void Abort()
        {
            _encodedTransfer.Abort(_asyncResult);
        }

        public System.IO.Stream GetRequestStream()
        {
            return _requestStream;
        }

        public IAsyncResult BeginGetResponse(AsyncCallback asyncResult, object args)
        {
            var dataToSend = new EncodingRequestHeader
            { 
                HttpMethod = _httpMethod,
                RequestUri = _requestUri,
                Version = _version,
                RequestHeaders = _requestHeaders
            };
            
            _asyncResult = _encodedTransfer.SendRequestAsync(new KeyValuePair<EncodingRequestHeader, EncodingRequestBody>(dataToSend,
                new EncodingRequestBody { Body = _requestStream.ToArray() }),
                (requestSent) => 
                {                    
                    _encodedTransfer.ReceiveResponseHeaderAsync(requestSent,
                        (headers) => {
                            if (headers.HasBody)
                            {
                                _encodedTransfer.ReceiveResponseBodyAsync(requestSent, (body) => 
                                {
                                    Debug.WriteLine("Returned body: " + body.Body.Length);
                                });
                            }
                        });
                });

            return _asyncResult;
        }

        public IProxyResponse EndGetResponse(IAsyncResult asyncResult)
        {
            var aResult = (IEncodedAsyncResult)asyncResult;

            var proxyResponse = new EncodedProxyResponse(aResult);

            if (!aResult.IsCompleted)
            {
                if (!aResult.AsyncWaitHandle.WaitOne(WaitTimeoutMSecs))
                {
                    _encodedTransfer.Abort(aResult);
                }
            }

            aResult.WaitForHeader();
            return proxyResponse;
        }

        private string GetRequestHeader(string name)
        {
            var header = _requestHeaders.FirstOrDefault(
                w => w.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
            return header != null ? header.Value : string.Empty;
        }

        private void SetRequestHeader(string name, string data)
        {
            var header = _requestHeaders.FirstOrDefault(
                w => w.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
            if (header == null)
                _requestHeaders.Add(new HttpHeader(name, data));
            else
            {
                header.Value = data;
            }
        }
    }
}
