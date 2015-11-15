using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProxyLanguage.Models;

namespace Proxy.Encoding
{
    [Serializable]
    public class EncodingRequestHeader
    {
        public Version Version;
        public string HttpMethod;
        public Uri RequestUri;
        public List<HttpHeader> RequestHeaders;
    }
}
