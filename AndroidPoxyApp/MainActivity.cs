using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Android.App;
using Android.Bluetooth;
using Android.Content;
using Android.Media;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Util;
using EndPointProxy;
using ERemoteProxy.ServiceLibrary;
using Java.Util;
using Proxy.Encoding;
using Stream = System.IO.Stream;

namespace AndroidPoxyApp
{
    [Activity(Label = "AndroidPoxyApp", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        public static string StartMark = "94DF4C4031914E67A140D5B0C9306E13";
        public static string EndMark = "C7C4AD4AE5E94B80916C215B3098B504";
        public const string ServiceKey = "B98D2D6D-7B9D-4BA0-BEE4-077E49A4AC94";
        const int TimeoutMSecs = 10000;
        const int BufferSize = 65536;
        private bool _bluetoothEnabled = false;
        private string _listenDeviceName = string.Empty;
        private BluetoothServerSocket _serverSocket;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            // Get our button from the layout resource,
            // and attach an event to it
            Button button = FindViewById<Button>(Resource.Id.StartStopButton);

            button.Click += delegate
            {
                _bluetoothEnabled = !_bluetoothEnabled && !string.IsNullOrWhiteSpace(_listenDeviceName);
                button.Text = _bluetoothEnabled ? "Stop" : "Start";
                if (_bluetoothEnabled)
                    StartListening();
                else StopListening();
            };

            AddBluetoothDevices();

            ConfigurationSingleton.Instance = new AndroidConfigurationManager();

            button.Text = "Listening automatically";
            _bluetoothEnabled = true;
            StartListening();
        }

        private void StopListening()
        {
            if (_serverSocket != null)
            {
                Log.Debug("IO", "Stop listening");
                _serverSocket.Close();
                _serverSocket.Dispose();
                _serverSocket = null;
            }
        }

        private void StartListening()
        {
            var bluetoothAdapter = BluetoothAdapter.DefaultAdapter;
            if (bluetoothAdapter != null && bluetoothAdapter.IsEnabled)
            {
                _serverSocket = bluetoothAdapter.ListenUsingInsecureRfcommWithServiceRecord("StreamData", UUID.FromString(ServiceKey));
                Log.Debug("IO", "Start listening");
                Task.Run(() =>
                {
                    while (_bluetoothEnabled && _serverSocket != null)
                    {
                        try
                        {
                            var socket = _serverSocket.Accept(TimeoutMSecs);
                            if (socket != null)
                            {
                                Log.Debug("IO", "Socket accepted");
                                try
                                {
                                    var readStream = new MemoryStream();
                                    while (socket.IsConnected && _bluetoothEnabled)
                                    {
                                        var buffer = new byte[BufferSize];
                                        var readSize = socket.InputStream.Read(buffer, 0, BufferSize);
                                        if (readSize == 0)
                                            continue;
                                        Log.Debug("IO", "Data read: " + readSize);

                                        readStream.Seek(readStream.Length, SeekOrigin.Begin);
                                        readStream.Write(buffer, 0, readSize);
                                        if (ProcessReadStream(readStream, socket.OutputStream))
                                        {
                                            socket.Close();
                                        }
                                    }
                                }
                                catch (Java.IO.IOException ioRead)
                                {
                                    Log.Debug("IO", "Read error: " + ioRead.Message);
                                }
                                finally
                                {
                                    socket.Close();
                                    socket.Dispose();
                                }
                            }
                        }
                        catch (Java.IO.IOException ioError)
                        {
                            Log.Debug("IO", "Still no connections: " + ioError.Message);

                        }
                    }
                });
            }
            bluetoothAdapter?.Dispose();
        }

        private bool ProcessReadStream(MemoryStream readStream, Stream outputStream)
        {
            while (true)
            {
                var inputString = System.Text.Encoding.ASCII.GetString(readStream.ToArray());
                var idxStart = inputString.IndexOf(StartMark);
                var idxEnd = inputString.IndexOf(EndMark, idxStart + 1);
                if (idxStart >= 0 && idxEnd > 0)
                {
                    readStream.Seek(0, SeekOrigin.Begin);
                    readStream.SetLength(0);
                    var copyArray =
                        System.Text.Encoding.ASCII.GetBytes(inputString.Substring(idxEnd + EndMark.Length,
                            inputString.Length - idxEnd - EndMark.Length));
                    readStream.Write(copyArray, 0, copyArray.Length);
                    var inputCommand = inputString.Substring(idxStart + StartMark.Length,
                        idxEnd - idxStart - StartMark.Length);

                    if (!string.IsNullOrEmpty(inputCommand))
                    {
                        RunOnUiThread(new Action(() =>
                        {
                            var logView = FindViewById<TextView>(Resource.Id.logView);
                            logView.Text = inputCommand + "\n";
                        }));

                        var request =
                            (BluetoothRequest)
                                new XmlSerializer(typeof (BluetoothRequest)).Deserialize(new StringReader(inputCommand));
                        ProcessRequest(request, outputStream);
                        return true;
                    }
                }
                else break;
            }

            return false;
        }

        private void AddBluetoothDevices()
        {
            var adapter = BluetoothAdapter.DefaultAdapter;
            if (adapter != null && adapter.IsEnabled)
            {
                FindViewById<Button>(Resource.Id.StartStopButton).Enabled = true;
                var pairedDevices = adapter.BondedDevices;
                foreach (var device in pairedDevices)
                {
                    var deviceButton = new RadioButton(this);
                    deviceButton.Text = device.Name;
                    deviceButton.Click += (sender, args) =>
                    {
                        _listenDeviceName = ((RadioButton) sender).Text;
                    };
                    FindViewById<RadioGroup>(Resource.Id.radioGroup).AddView(deviceButton);
                }
            }
            else FindViewById<Button>(Resource.Id.StartStopButton).Enabled = false;
            adapter?.Dispose();
        }

        public void ProcessRequest(BluetoothRequest request, Stream outputStream)
        {
            byte[] response = null;
            try
            {
                if (request.HttpMethod == "GET")
                {
                    var key = request.Key;
                    var body = request.IsBody;
                    var position = request.Position;
                    var size = request.Size;
                    var buffer = new ERemoteHandler().Get(key, body, position, size);

                    outputStream.Write(buffer, 0, buffer.Length);
                }
                else if (request.HttpMethod == "POST")
                {
                    response = new ERemoteHandler().Post(request.Body);
                    outputStream.Write(response, 0, response.Length);
                }
                else if (request.HttpMethod == "DELETE")
                {
                    var key = request.Key;
                    new ERemoteHandler().Delete(key);
                    response = System.Text.Encoding.ASCII.GetBytes("Deleted");
                }
                else
                    throw new ArgumentOutOfRangeException("Unknown HTTP Method: " + request.HttpMethod);
            }
            catch (Exception errorException)
            {
                Log.Error("IO", errorException.ToString());
                response = System.Text.Encoding.ASCII.GetBytes("501 " + errorException.ToString());
            }
            outputStream.Write(response, 0, response.Length);
            var endMarkBytes = System.Text.Encoding.ASCII.GetBytes(EndMark);
            outputStream.Write(endMarkBytes, 0, endMarkBytes.Length);
            outputStream.Flush();
        }
    }
}

