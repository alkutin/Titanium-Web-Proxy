using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proxy.Filters.Model
{
    public class RoutesConfig
    {
        public class BluetoothConfig
        {
            public string Device;
            public bool Enabled;
        }

        public BluetoothConfig Bluetooth;
        public string[] Forbiden;
        public Dictionary<string, string[]> Proxy;
    }
}
