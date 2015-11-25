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

        static RoutesFilter()
        {
            Init();
        }

        public RoutesFilter(IEncodedTransfer next)
        {
            _next = next;            
        }

        private static void Init()
        {
            _path = Path.GetFullPath(ConfigurationManager.AppSettings["Routes"]);

            _enableForbiden = bool.Parse(ConfigurationManager.AppSettings["EnableForbiden"]);
            _enableSelectiveProxy = bool.Parse(ConfigurationManager.AppSettings["EnableSelectiveProxy"]);

            try
            {
                LoadRoutesFile();
             
                _watcher = new FileSystemWatcher(Path.GetDirectoryName(_path), Path.GetFileName(_path));
                _watcher.Changed += RoutesFileChanged;                
                _watcher.EnableRaisingEvents = true;
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

        private static void RoutesFileChanged(object sender, FileSystemEventArgs e)
        {
            Task.Run(() => { LoadRoutesFile(); });
        }

        public void Abort(IEncodedAsyncResult requestAsyncResult)
        {
            _next.Abort(requestAsyncResult);
        }

        public void ReceiveResponseBodyAsync(IEncodedAsyncResult requestAsyncResult, Action<EncodingResponseBody> onReceiveBody)
        {
            _next.ReceiveResponseBodyAsync(requestAsyncResult, onReceiveBody);
        }

        public void ReceiveResponseHeaderAsync(IEncodedAsyncResult requestAsyncResult, Action<EncodingResponseHeader> onReceivedResponse)
        {
            _next.ReceiveResponseHeaderAsync(requestAsyncResult, onReceivedResponse);
        }

        public IEncodedAsyncResult SendRequestAsync(KeyValuePair<EncodingRequestHeader, EncodingRequestBody> request, Action<IEncodedAsyncResult> onComplete)
        {
            if (_enableForbiden)
            {
                foreach (var url in _config.Forbiden)
                    if (request.Key.RequestUri.Host == url)
                    {
                        var rez = new EncodingAsyncResult
                        {
                            Key = Guid.Empty,
                            RequestHeaders = request.Key,
                            ResponseBody = new EncodingResponseBody { Body = new byte[0] },
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

            return _next.SendRequestAsync(request, onComplete);
        }
    }
}
