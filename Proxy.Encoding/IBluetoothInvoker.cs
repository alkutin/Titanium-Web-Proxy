using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proxy.Encoding
{
    public interface IBluetoothInvoker
    {
        byte[] SendData(string deviceName, BluetoothRequest request);
    }

    public static class BluetoothInvokerSingleton
    {
        public static IBluetoothInvoker Instance { get; set; }
    }

}
