using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EndPointProxy
{
    public class EndpointWaitResult : IAsyncResult
    {
        private HttpWebRequest _proxyRequest;
        private object _args;
        private AsyncCallback _asyncResultHandler;
        private WebResponse _webResponse;
        private ManualResetEvent _waitEvent = new ManualResetEvent(false);

        IAsyncResult InnerResult { get; set; }

        public bool IsCompleted
        {
            get
            {
                return  InnerResult.IsCompleted;
            }
        }

        public WaitHandle AsyncWaitHandle
        {
            get
            {
                return _waitEvent;// InnerResult.AsyncWaitHandle;
            }
        }

        public object AsyncState
        {
            get
            {
                return _args;// InnerResult.AsyncState;
            }
        }

        public bool CompletedSynchronously
        {
            get
            {
                return InnerResult.CompletedSynchronously;
            }
        }

        public EndpointWaitResult(HttpWebRequest proxyRequest)
        {
            _proxyRequest = proxyRequest;
        }

        public IAsyncResult BeginGetResponse(AsyncCallback asyncResult, object args)
        {
            _args = args;
            _asyncResultHandler = asyncResult;
            //var task = new Task((state) =>
            //{
                InnerResult = _proxyRequest.BeginGetResponse(HandleAsyncResponse, args);
            //}, this);
            //task.Start();
            new Task((state) => {
                InnerResult.AsyncWaitHandle.WaitOne();
                try
                {
                    _webResponse = _proxyRequest.EndGetResponse(InnerResult);
                }
                catch (WebException webEx)
                {
                    Debug.WriteLine(webEx.ToString());
                    _webResponse = webEx.Response as HttpWebResponse;
                }
                finally
                {
                    _waitEvent.Set();
                }
            }, this).Start();

            return this;
        }

        public static WebResponse EndGetResponse(IAsyncResult asyncResult)
        {
            var state = (EndpointWaitResult)asyncResult;
            state._waitEvent.WaitOne();           
            return state._webResponse;
        }

        void HandleAsyncResponse(IAsyncResult asynchronousResult)
        {
            _asyncResultHandler(this);
        }
    }
}
