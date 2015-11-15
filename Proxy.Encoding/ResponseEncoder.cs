using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proxy.Encoding
{
    public class ResponseEncoder
    {
        private IEncodedTransfer _transfer;
        public ResponseEncoder(IEncodedTransfer transfer)
        {
            _transfer = transfer;
        }
    }
}
