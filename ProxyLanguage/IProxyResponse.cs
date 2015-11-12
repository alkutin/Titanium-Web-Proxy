using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ProxyLanguage
{
    public interface IProxyResponse
    {
        Version ProtocolVersion { get; }
        HttpStatusCode StatusCode { get; }
        string StatusDescription { get; }
        string ContentEncoding { get;  }
        IDictionary<string, string> Headers { get; }

        string GetResponseHeader(string name);
        Encoding GetEncoding();
        void Close();
        Stream GetResponseStream();
    }
}
