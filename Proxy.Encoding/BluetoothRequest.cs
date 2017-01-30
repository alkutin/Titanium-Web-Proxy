using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proxy.Encoding
{
    public class BluetoothRequest
    {
        public string HttpMethod { get; set; }
        public Guid Key { get; set; }
        public long Position { get; set; }
        public long Size { get; set; }
        public bool IsBody { get; set; }
        public byte[] Body { get; set; }
    }
}
