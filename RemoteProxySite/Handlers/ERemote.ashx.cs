using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Proxy.Encoding;
using RemoteProxySite.Models;
using ProxyLanguage.Models;
using System.Globalization;

namespace RemoteProxySite.Handlers
{
    /// <summary>
    /// Summary description for ERemote
    /// </summary>
    public class ERemote : IHttpHandler
    {
        private static System.Collections.Concurrent.ConcurrentDictionary<Guid, ResponseTuple> _sessions =
            new System.Collections.Concurrent.ConcurrentDictionary<Guid, ResponseTuple>();
        private Proxy.Encoding.Encoder _encoder;

        public ERemote()
        {
            _encoder = new Proxy.Encoding.Encoder(Encoding.ASCII.GetBytes(ConfigurationManager.AppSettings["Key"]), Encoding.ASCII.GetBytes(ConfigurationManager.AppSettings["Vector"]));
        }

        public void ProcessRequest(HttpContext context)
        {
            try
            {
                context.Response.StatusCode = 200;
                context.Response.ContentType = "application/content-stream";
                context.Response.BufferOutput = true;

                if (context.Request.HttpMethod == "GET")
                {
                    var key = Guid.Parse(context.Request.Params["Key"]);
                    var body = bool.Parse(context.Request.Params["B"]);
                    var position = int.Parse(context.Request.Params["P"]);
                    var size = int.Parse(context.Request.Params["S"]);
                    var buffer = Get(key, body, position, size);
                    
                    context.Response.BinaryWrite(buffer);
                } 
                else if (context.Request.HttpMethod == "POST")
                {
                    var stream = new MemoryStream();
                    context.Request.GetBufferedInputStream().CopyTo(stream);
                    context.Response.BinaryWrite(Post(stream.ToArray()));
                } 
                else if (context.Request.HttpMethod == "DELETE")
                {
                    var key = Guid.Parse(context.Request.Params["key"]);
                    Delete(key);
                    context.Response.Write("Deleted");
                }
                else                 
                    throw new ArgumentOutOfRangeException("Unknown HTTP Method: " + context.Request.HttpMethod);
            }
            catch (Exception errorException)
            {
                Debug.WriteLine(errorException.ToString());
                context.Response.StatusCode = 501;
                context.Response.ContentType = "application/text-plain";
                context.Response.Write(errorException.ToString());
            }
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }

        public byte[] Get(Guid key, bool body, int position, int size)
        {
            ResponseTuple info;
            var readPosition = 0;
            if (_sessions.TryGetValue(key, out info))
            {
                EncodingResponseBody eBody = null;
                if (body)
                {
                    info.AsyncResult.WaitForBody();
                    if (position == 0 && size == 0)
                    {
                        eBody = info.ResponseBody;
                    }
                    else
                    {
                        var bodyStream = info.ResponseBody.GetBody();
                        if (bodyStream.Position != position)
                        {
                            var newPosition = bodyStream.Seek(position, SeekOrigin.Begin);
                            if (newPosition != position)
                                throw new ArgumentOutOfRangeException("position");
                        }
                        readPosition = position;
                        var buffer = new byte[size];
                        var readSize = bodyStream.Read(buffer, 0, size);
                        var readBuffer = readSize != size ? new byte[readSize] : buffer;
                        if (readSize != size)
                            Array.Copy(buffer, 0, readBuffer, 0, readSize);

                        eBody = new PlainEncodingResponseBody
                        {
                            Position =  readPosition,
                            PlainBody = readBuffer
                        };
                    }

                    if (size == 0 || size != eBody.GetBody().Length)
                    {
                        Task.Delay(10000).ContinueWith((task) =>
                        {
                            ((IDisposable)info.AsyncResult).Dispose();
                            ResponseTuple removedInfo;
                            _sessions.TryRemove(key, out removedInfo);
                        });
                    }
                }
                else info.AsyncResult.WaitForHeader();

                return _encoder.Encode(body ? (object)eBody : info.ResponseHeader);
            }
            else return new byte[0];
        }

        // POST: api/ERemote
        public byte[] Post([FromBody]byte[] content)
        {
            var request = _encoder.Decode<EncodingRequest>(content);

            var encoder = new ResponseEncoder();
            var info = new ResponseTuple { RequestHeader = request.Header, RequestBody = request.Body };

            if (request.Header.HttpMethod == "POST" && !request.Header.RequestHeaders.Any(w => w.Name == "Content-Length"))
                request.Header.RequestHeaders.Add(new HttpHeader { Name = "Content-Length", Value = request.Body.Body.Length.ToString(CultureInfo.InvariantCulture) });

            info.AsyncResult = encoder.SendRequestAsync(new KeyValuePair<EncodingRequestHeader, EncodingRequestBody>(request.Header, request.Body),
                (complete) =>
                {
                });

            encoder.ReceiveResponseHeaderAsync(info.AsyncResult, (responseHeaders) =>
            {
                info.ResponseHeader = responseHeaders;
                var file = string.Empty;                

                if (!string.IsNullOrEmpty(responseHeaders.ETag))
                {
                    file = Path.Combine(Path.GetTempPath(), "ERemoteCache",
                        Encoding.ASCII.GetBytes(responseHeaders.ETag).GetMD5().Replace("/", "_").Replace("\\", "_").Replace("\"", "_")) + ".tmp";
                    try
                    {
                        if (File.Exists(file))
                        {
                            var body = new PlainEncodingResponseBody { PlainBody = File.ReadAllBytes(file) };
                            info.ResponseBody = body;
                            info.ResponseHeader.ETag = body.PlainBody.GetMD5();
                            info.ResponseHeader.ResponseHeaders.SetHeader("Content-Length", body.PlainBody.Length.ToString());
                        }
                    }
                    catch (Exception error)
                    {
                        Trace.TraceError(error.ToString());
                    }
                }

                if (info.ResponseBody == null)
                    encoder.ReceiveResponseBodyAsync(info.AsyncResult, (responseBody) =>
                    {
                        //info.ResponseHeader.ETag = responseBody.PlainBody.GetMD5();
                        info.ResponseBody = responseBody;
                        
                        try
                        {
                            if (!string.IsNullOrEmpty(file) && responseBody.GetBody() is MemoryStream)
                            {
                                var folder = Path.GetDirectoryName(file);
                                if (!Directory.Exists(folder))
                                    Directory.CreateDirectory(folder);
                                File.WriteAllBytes(file, ((MemoryStream)info.ResponseBody.GetBody()).ToArray());
                            }
                        }
                        catch(Exception error)
                        {
                            Trace.TraceError(error.ToString());
                        }
                    });
            });

            _sessions.TryAdd(info.AsyncResult.Key, info);
            return _encoder.Encode(info.AsyncResult.Key);
        }

        public void Delete(Guid key)
        {
            ResponseTuple removedInfo;
            if (_sessions.TryRemove(key, out removedInfo))
            {
                ((IDisposable)removedInfo.AsyncResult).Dispose();
            }
        }
    }
}