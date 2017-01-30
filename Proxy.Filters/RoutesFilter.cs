using Proxy.Encoding;
using Proxy.Filters.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using ProxyLanguage.Models;
using System.Diagnostics;

namespace Proxy.Filters
{
    public class RoutesFilter : IEncodedTransfer
    {
        private IEncodedTransfer _next;
        private static bool _enableForbiden;
        private static bool _enableSelectiveProxy;
        private static FileSystemWatcher _watcher;
        private static string _path;
        private static RoutesConfig _config;
        private IEncodedTransfer _skip;

        private IEncodedTransfer _current;
        private IEncodedBluetoothTransfer _bluetoothNext;

        static RoutesFilter()
        {

        }

        public RoutesFilter(IEncodedTransfer next, IEncodedBluetoothTransfer bluetoothNext, IEncodedTransfer skip)
        {
            _next = next;
            _bluetoothNext = bluetoothNext;
            _skip = skip;

            _current = _skip;

            _bluetoothNext.SetRemoteDevice(_config.Bluetooth.Device);
        }

        public static void Init()
        {
            _path = Path.GetFullPath(ConfigurationManager.AppSettings["Routes"]);

            _enableForbiden = bool.Parse(ConfigurationManager.AppSettings["EnableForbiden"]);
            _enableSelectiveProxy = bool.Parse(ConfigurationManager.AppSettings["EnableSelectiveProxy"]);

            try
            {
                LoadRoutesFile();

                _watcher = new FileSystemWatcher(Path.GetDirectoryName(_path), Path.GetFileNameWithoutExtension(_path) + ".*");
                _watcher.Changed += RoutesFileChanged;
                _watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size;
                _watcher.EnableRaisingEvents = true;
                Console.WriteLine("Watching for {0} at {1}", _watcher.Filter, _watcher.Path);
                //WatchRoutes();
            }
            catch(Exception error)
            {
                Console.WriteLine(error.ToString());
            }
        }

        private static void LoadRoutesFile()
        {
            var config = new JavaScriptSerializer().Deserialize<RoutesConfig>(File.ReadAllText(_path));
            _config = config;
        }

        private static void WatchRoutes()
        {
            Task.Run(() =>
            {
                var rez = _watcher.WaitForChanged(WatcherChangeTypes.Changed, 3000);
                if (!rez.TimedOut)
                {
                    Task.Run(() =>
                    {
                        Console.WriteLine("Routes changed event");
                        LoadRoutesFile();
                    });
                }
                WatchRoutes();
            });
        }

        private static void RoutesFileChanged(object sender, FileSystemEventArgs e)
        {
            Console.WriteLine("Routes File changed");
            Task.Delay(1000).ContinueWith((task) => { LoadRoutesFile(); });
        }

        public void Abort(IEncodedAsyncResult requestAsyncResult)
        {
            _current.Abort(requestAsyncResult);
        }

        public void ReceiveResponseBodyAsync(IEncodedAsyncResult requestAsyncResult, Action<EncodingResponseBody> onReceiveBody)
        {
            _current.ReceiveResponseBodyAsync(requestAsyncResult, onReceiveBody);
        }

        public void ReceiveResponseHeaderAsync(IEncodedAsyncResult requestAsyncResult, Action<EncodingResponseHeader> onReceivedResponse)
        {
            _current.ReceiveResponseHeaderAsync(requestAsyncResult, onReceivedResponse);
        }

        public IEncodedAsyncResult SendRequestAsync(KeyValuePair<EncodingRequestHeader, EncodingRequestBody> request, Action<IEncodedAsyncResult> onComplete)
        {
            if (_enableForbiden)
            {
                foreach (var url in _config.Forbiden)
                    if (request.Key.RequestAsUri.Host == url)
                    {
                        var rez = new EncodingAsyncResult
                        {
                            Key = Guid.Empty,
                            RequestHeaders = request.Key,
                            ResponseBody = new PlainEncodingResponseBody { PlainBody = new byte[0] },
                            ResponseHeaders = new EncodingResponseHeader {
                                ContentEncoding = string.Empty,
                                ETag = string.Empty,
                                HttpCode = System.Net.HttpStatusCode.Forbidden,
                                HttpDescription = "Forbiden host",
                                ResponseHeaders = new List<ProxyLanguage.Models.HttpHeader>()
                            }
                        };
                        rez.ResponseHeaders.ResponseHeaders.SetHeader("Content-Length", "0");
                        Task.Run(() => { onComplete(rez); });
                        return rez;
                    };
            }

            var oldColor = Console.ForegroundColor;
            if (_enableSelectiveProxy)
            {
                //Console.WriteLine(request.Key.RequestUri.Host);
                var agentFound = false;
                var userAgent = request.Key.RequestHeaders.GetHeader("User-Agent");
                foreach (var agent in _config.Proxy)
                {
                    if (agent.Key.Equals(userAgent))
                    {
                        agentFound = true;
                        if (agent.Value.Length == 0
                            || agent.Value.Any(w => w.StartsWith(@".") ? ("." + request.Key.RequestAsUri.Host).EndsWith(w) : w.Equals(request.Key.RequestAsUri.Host)))
                        {
                            _current = _config.Bluetooth.Enabled ? _bluetoothNext : _next;
                            break;
                        }
                    }
                }

                if (!agentFound)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Unknown User Agent: " + userAgent);
                    Console.ForegroundColor = oldColor;
                }

            }
            else _current = _config.Bluetooth.Enabled ? _bluetoothNext : _next;

            Console.ForegroundColor = ConsoleColor.Yellow;
            if (_current == (_config.Bluetooth.Enabled ? _bluetoothNext : _next))
                Console.WriteLine("Next: " + request.Key.RequestAsUri.Host);
            else Console.WriteLine("Skip: " + request.Key.RequestAsUri.Host);
            Console.ForegroundColor = oldColor;

            return _current.SendRequestAsync(request, onComplete);
        }
    }
}
