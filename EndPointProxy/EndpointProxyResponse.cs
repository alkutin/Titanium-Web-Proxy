using ProxyLanguage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections;
using EndPointProxy.Extensions;

namespace EndPointProxy
{
    public class EndpointProxyResponse : IProxyResponse
    {
        private Dictionary<string, string> _headers;
        private HttpWebResponse _proxyResponse;

        public EndpointProxyResponse(HttpWebResponse proxyResponse)
        {
            _proxyResponse = proxyResponse;
        }

        public string ContentEncoding
        {
            get
            {
                return _proxyResponse != null ? _proxyResponse.ContentEncoding : string.Empty;
            }
        }

        public IDictionary<string, string> Headers
        {
            get
            {
                if (_headers == null && _proxyResponse != null)
                    _headers = _proxyResponse.Headers.AllKeys.ToDictionary(k => k, v => _proxyResponse.Headers[v]);
                return _headers;
            }               
        }

        public Version ProtocolVersion
        {
            get
            {
                return _proxyResponse != null ? _proxyResponse.ProtocolVersion : new Version(1, 0);
            }
        }

        public HttpStatusCode StatusCode
        {
            get
            {
                return _proxyResponse != null ? _proxyResponse.StatusCode : HttpStatusCode.ServiceUnavailable;
            }
        }

        public string StatusDescription
        {
            get
            {
                return _proxyResponse != null ? _proxyResponse.StatusDescription : "Service Unavaiable";
            }
        }

        public void Close()
        {
            if (_proxyResponse != null)
                _proxyResponse.Close();
        }

        public Encoding GetEncoding()
        {
            return _proxyResponse != null ? _proxyResponse.GetEncoding() : Encoding.ASCII;
        }

        public string GetResponseHeader(string name)
        {
            if (Headers == null)
                return string.Empty;

            foreach (var header in Headers)
            {
                if (header.Key.Equals(name, StringComparison.InvariantCultureIgnoreCase))
                    return header.Value;
            }
            
            return string.Empty;
        }

        public Stream GetResponseStream()
        {
            return _proxyResponse != null ? (Stream)new EndpointProxyStream(_proxyResponse.GetResponseStream()) : 
                new MemoryStream();
        }
    }
}
