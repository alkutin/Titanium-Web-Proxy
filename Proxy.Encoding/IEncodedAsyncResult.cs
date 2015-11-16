using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProxyLanguage.Models;

namespace Proxy.Encoding
{
    public interface IEncodedAsyncResult : IAsyncResult
    {
        Guid Key { get; set; }
        EncodingRequestHeader RequestHeaders { get; set; }
        EncodingResponseHeader ResponseHeaders { get; set; }
        EncodingResponseBody ResponseBody { get; set; }

        void SetAsyncState(object asyncState);
        void WaitForHeader();
        void WaitForBody();
    }
}
