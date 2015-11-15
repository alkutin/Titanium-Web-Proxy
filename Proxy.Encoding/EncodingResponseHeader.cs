using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using ProxyLanguage.Models;

namespace Proxy.Encoding
{
    [Serializable]
    public class EncodingResponseHeader
    {
        public bool HasBody;
        public HttpStatusCode HttpCode;
        public string HttpDescription;
        public List<HttpHeader> ResponseHeaders;

        public string ContentEncoding { get; set; }
    }
}
