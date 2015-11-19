using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Proxy.Encoding;
using RemoteProxySite.Models;

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
                context.Response.ContentType = "application/content-stream";

                if (context.Request.HttpMethod == "GET")
                {
                    
                } else if (context.Request.HttpMethod == "POST")
                {
                    
                } else if (context.Request.HttpMethod == "DELETE")
                {
                    
                }

                context.Response.Write("Hello World");
                throw new ArgumentOutOfRangeException("Unknown HTTP Method: " + context.Request.HttpMethod);
            }
            catch (Exception errorException)
            {
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

        public byte[] Get(Guid key, bool body)
        {
            ResponseTuple info;
            if (_sessions.TryGetValue(key, out info))
            {
                if (body)
                {
                    info.AsyncResult.WaitForBody();
                    Task.Delay(1000).ContinueWith((task) =>
                    {
                        ((IDisposable)info.AsyncResult).Dispose();
                        ResponseTuple removedInfo;
                        _sessions.TryRemove(key, out removedInfo);
                    });
                }
                else info.AsyncResult.WaitForHeader();

                return _encoder.Encode(body ? (object)info.ResponseBody : info.ResponseHeader);
            }
            else return new byte[0];
        }

        // POST: api/ERemote
        public byte[] Post([FromBody]byte[] content)
        {
            var request = _encoder.Decode<EncodingRequest>(content);

            var encoder = new ResponseEncoder();
            var info = new ResponseTuple { RequestHeader = request.Header, RequestBody = request.Body };

            info.AsyncResult = encoder.SendRequestAsync(new KeyValuePair<EncodingRequestHeader, EncodingRequestBody>(request.Header, request.Body),
                (complete) =>
                {
                });

            encoder.ReceiveResponseHeaderAsync(info.AsyncResult, (responseHeaders) =>
            {
                info.ResponseHeader = responseHeaders;
                encoder.ReceiveResponseBodyAsync(info.AsyncResult, (responseBody) =>
                {
                    info.ResponseBody = responseBody;
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