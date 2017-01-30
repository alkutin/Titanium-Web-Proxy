using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using EndPointProxy;
using ProxyLanguage.Models;

namespace Proxy.Encoding
{
    public interface IEncodedBluetoothTransfer : IEncodedTransfer
    {
        void SetRemoteDevice(string deviceName);
    }

    public class EncodedBluetoothTransfer : IEncodedBluetoothTransfer
    {
        static ConcurrentDictionary<Guid, EncodingAsyncResult> _sessions =
            new ConcurrentDictionary<Guid, EncodingAsyncResult>();

        private Encoder _encoder;
        private string _deviceName;

        public byte[] Key
        {
            get { return System.Text.Encoding.ASCII.GetBytes(ConfigurationSingleton.Instance.Key); }
        }

        public byte[] Vector
        {
            get { return System.Text.Encoding.ASCII.GetBytes(ConfigurationSingleton.Instance.Vector); }
        }

        public EncodedBluetoothTransfer()
        {
            _encoder = new Encoder(Key, Vector);
        }

        public void Abort(IEncodedAsyncResult requestAsyncResult)
        {
            BluetoothInvokerSingleton.Instance.SendData(_deviceName, new BluetoothRequest
            {
                HttpMethod = "DELETE",
                Key = requestAsyncResult.Key
            });
            EncodingAsyncResult removedItem;
            if (_sessions.TryRemove(requestAsyncResult.Key, out removedItem))
                removedItem.Dispose();
        }

        public void SetRemoteDevice(string deviceName)
        {
            _deviceName = deviceName;
        }

        public void ReceiveResponseBodyAsync(IEncodedAsyncResult requestAsyncResult,
            Action<EncodingResponseBody> onReceiveBody)
        {
            var folder = Path.Combine(Path.GetTempPath(), "ERemoteCache");
            var eTag = requestAsyncResult.ResponseHeaders.ETag.Replace("/", "_").Replace("\\", "_").Replace("\"", "_");

            if (!string.IsNullOrEmpty(eTag))
            {
                var file = Path.Combine(folder, eTag) + ".$$$";
                if (File.Exists(file))
                {
                    requestAsyncResult.ResponseBody = new PlainEncodingResponseBody
                    {
                        PlainBody = File.ReadAllBytes(file)
                    };

                    Task.Delay(5000).ContinueWith(t =>
                    {
                        EncodingAsyncResult removedItem;

                        if (_sessions.TryRemove(requestAsyncResult.Key, out removedItem))
                            removedItem.Dispose();
                    });

                    if (onReceiveBody != null)
                    {
                        onReceiveBody(requestAsyncResult.ResponseBody);
                    }

                    Debug.WriteLine(string.Concat("Returned from cache: ", eTag));
                    return;
                }
            }

            long contentLength = 0;
            if (!long.TryParse(requestAsyncResult.ResponseHeaders.ResponseHeaders.GetHeader("Content-Length"),
                out contentLength))
                contentLength = int.MaxValue;

            requestAsyncResult.ResponseBody = new StreamEncodingResponseBody
            {
                BodyStream = new ReadByEventStream(
                    requestAsyncResult.ResponseHeaders.HasBody
                        ? contentLength
                        : 0,
                    new Func<long, long, byte[]>((position, blockSize) =>
                    {

                        var response = BluetoothInvokerSingleton.Instance.SendData(_deviceName, new BluetoothRequest
                        {
                           HttpMethod = "GET",
                           Key = requestAsyncResult.Key,
                           IsBody = true,
                           Position = position,
                           Size = blockSize
                        });

                        var data = _encoder.Decode<PlainEncodingResponseBody>(response);
                        //Debug.Write(data != null ? System.Text.Encoding.ASCII.GetString(data.PlainBody) : string.Empty);
                        if (data.Position != position)
                            throw new IOException(string.Format("Bad position: {0} instead of {1}", data.Position,
                                position));

                        if (position == 0 && blockSize != data.PlainBody.Length && !string.IsNullOrEmpty(eTag))
                        {
                            try
                            {
                                var file = Path.Combine(folder, eTag);
                                var fullFolder = Path.GetDirectoryName(file);
                                if (!Directory.Exists(fullFolder))
                                    Directory.CreateDirectory(fullFolder);
                                File.WriteAllBytes(file, data.PlainBody);
                            }
                            catch (Exception error)
                            {
                                Trace.TraceError(error.ToString());
                            }

                            Task.Delay(5000).ContinueWith(t =>
                            {
                                EncodingAsyncResult removedItem;

                                if (_sessions.TryRemove(requestAsyncResult.Key, out removedItem))
                                    removedItem.Dispose();
                            });
                        }

                        return data != null ? data.PlainBody : new byte[0];
                    })
                    )
            };

            if (onReceiveBody != null)
            {
                onReceiveBody(requestAsyncResult.ResponseBody);
            }
        }

        public void ReceiveResponseHeaderAsync(IEncodedAsyncResult requestAsyncResult,
            Action<EncodingResponseHeader> onReceivedResponse)
        {
            var responseTask = Task<byte[]>.Run(() =>
            {
                return BluetoothInvokerSingleton.Instance.SendData(_deviceName, new BluetoothRequest
                {
                   HttpMethod = "GET",
                   Key = requestAsyncResult.Key,
                   IsBody = false,
                   Position = 0,
                   Size = 0
                });
            }
            );

            responseTask.ContinueWith(task =>
            {
                var data = _encoder.Decode<EncodingResponseHeader>(task.Result);
                requestAsyncResult.ResponseHeaders = data;
                onReceivedResponse?.Invoke(data);
            });
        }

        public IEncodedAsyncResult SendRequestAsync(KeyValuePair<EncodingRequestHeader, EncodingRequestBody> request,
            Action<IEncodedAsyncResult> onComplete)
        {
            var encodingRequest = _encoder.Encode(new EncodingRequest {Header = request.Key, Body = request.Value});

            var result = BluetoothInvokerSingleton.Instance.SendData(_deviceName, new BluetoothRequest
            {
                HttpMethod = "POST",
                Body = encodingRequest
            });

            var key = _encoder.Decode<Guid>(result);
            var asyncResult = new EncodingAsyncResult {Key = key, RequestHeaders = request.Key};
            _sessions.TryAdd(key, asyncResult);

            if (onComplete != null)
                Task.Run(() => { onComplete(asyncResult); });

            return asyncResult;
        }
    }
}
