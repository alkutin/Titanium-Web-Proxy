using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proxy.Encoding
{
    [Serializable]
    public class EncodingRequest
    {
        public EncodingRequestHeader Header;
        public EncodingRequestBody Body;
    }
}
