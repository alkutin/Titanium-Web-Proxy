using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ProxyLanguage;

namespace Proxy.Encoding
{
    public class EncodedProxyResponse : IProxyResponse
    {
        private IEncodedAsyncResult _asyncResult;
        private Stream _stream;

        public EncodedProxyResponse(IEncodedAsyncResult asyncResult)
        {
            _asyncResult = asyncResult;
        }

        public Version ProtocolVersion
        {
            get { return Version.Parse(_asyncResult.RequestHeaders.Version); }
        }

        public System.Net.HttpStatusCode StatusCode
        {
            get { return _asyncResult.ResponseHeaders.HttpCode; }
        }

        public string StatusDescription
        {
            get { return _asyncResult.ResponseHeaders.HttpDescription; }
        }

        public string ContentEncoding
        {
            get {
                var contentType = GetResponseHeader("Content-Type");
                if (!string.IsNullOrEmpty(contentType))
                {
                    var encodingSplit = contentType.Split('=');
                    if (encodingSplit.Length == 2 && encodingSplit[0].ToLower().Trim() == "charset")
                    {
                        return encodingSplit[1];
                    }
                }

                return string.Empty;
            }
        }

        public IDictionary<string, string> Headers
        {
            get { return _asyncResult.ResponseHeaders.ResponseHeaders.ToDictionary(k => k.Name, v => v.Value); }
        }

        public string GetResponseHeader(string name)
        {
            var header = _asyncResult.ResponseHeaders.ResponseHeaders.FirstOrDefault(
                w => w.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
            return header != null ? header.Value : string.Empty;
        }

        public System.Text.Encoding GetEncoding()
        {
            var encoding = ContentEncoding;
            return string.IsNullOrEmpty(encoding) ? System.Text.Encoding.GetEncoding("ISO-8859-1")
                : System.Text.Encoding.GetEncoding(encoding);
        }

        public void Close()
        {
            
        }

        public System.IO.Stream GetResponseStream()
        {
            if (_stream == null)
            {
                _asyncResult.WaitForBody();
                _stream = _asyncResult.ResponseBody.GetBody();
            }

            return _stream;
        }
    }
}
