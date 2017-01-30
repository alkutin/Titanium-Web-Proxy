using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using InTheHand.Net;
using InTheHand.Net.Sockets;
using Proxy.Encoding;

namespace Titanium.Web.Proxy
{
    public class BluetoothInvoker : IBluetoothInvoker
    {
        public static byte[] StartMark = System.Text.Encoding.ASCII.GetBytes("94DF4C4031914E67A140D5B0C9306E13");
        public static byte[] EndMark = System.Text.Encoding.ASCII.GetBytes("C7C4AD4AE5E94B80916C215B3098B504");
        public static Guid ServiceKey = new Guid("B98D2D6D-7B9D-4BA0-BEE4-077E49A4AC94");

        public byte[] SendData(string deviceName, BluetoothRequest request)
        {
            var client = new BluetoothClient();
            var device = client.DiscoverDevices(short.MaxValue, true, true, false, false).FirstOrDefault(w => w.DeviceName == deviceName);
            if (device == null)
            {
                Console.WriteLine("Device {0} not found.", deviceName);
                return new byte[0];
            }

            var address = new BluetoothEndPoint(device.DeviceAddress, ServiceKey);
            try
            {
                client.Connect(address);
                try
                {
                    var requestStream = new MemoryStream();
                    var responseStream = new MemoryStream();
                    new XmlSerializer(typeof (BluetoothRequest)).Serialize(requestStream, request);
                    var buffer = requestStream.ToArray();
                    using (var stream = client.GetStream())
                    {
                        stream.Write(StartMark, 0, StartMark.Length);
                        stream.Write(buffer, 0, buffer.Length);
                        stream.Write(EndMark, 0, EndMark.Length);
                        stream.Flush();

                        while (client.Connected)
                        {
                            var readBuffer = new byte[65536];
                            var readSize = stream.Read(readBuffer, 0, buffer.Length);
                            responseStream.Write(readBuffer, 0, readSize);

                            var data = responseStream.ToArray();
                            var endMarkOk = true;
                            var idxStart = Math.Max(0, data.Length - EndMark.Length);
                            for (var i = 0; i < EndMark.Length; i++)
                            {
                                if (data[idxStart + i] != EndMark[i])
                                {
                                    endMarkOk = false;
                                    break;
                                }
                            }

                            if (endMarkOk)
                            {
                                responseStream.SetLength(responseStream.Length - EndMark.Length);
                                break;
                            }
                        }
                        return responseStream.ToArray();
                    }
                }
                finally
                {
                    client.Close();
                }
            }
            catch (Exception error)
            {
                Console.WriteLine(error.ToString());
                return new byte[0];
            }
        }
    }
}
