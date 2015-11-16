using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Proxy.Encoding
{
    public class EncodingAsyncResult : IEncodedAsyncResult, IAsyncResult
    {
        ManualResetEvent _headerReceived = new ManualResetEvent(false);
        ManualResetEvent _bodyReceived = new ManualResetEvent(false);
        private EncodingResponseHeader _responseHeaders;
        private EncodingResponseBody _responseBody;

        public object AsyncState
        {
            get
            {
                return null;
            }
        }

        public WaitHandle AsyncWaitHandle
        {
            get
            {
                return _headerReceived;
            }
        }

        public bool CompletedSynchronously
        {
            get
            {
                return false;
            }
        }

        public bool IsCompleted
        {
            get
            {
                return _headerReceived.WaitOne(0);
            }
        }

        public Guid Key
        {
            get; set;
        }

        public EncodingRequestHeader RequestHeaders
        {
            get;
            set;
        }

        public EncodingResponseBody ResponseBody
        {
            get
            {
                WaitForBody();
                return _responseBody;
            }

            set
            {
                _responseBody = value;
                _bodyReceived.Set();
            }
        }
        
        public EncodingResponseHeader ResponseHeaders
        {
            get
            {
                WaitForHeader();
                return _responseHeaders;
            }

            set
            {
                _responseHeaders = value;
                _headerReceived.Set();
            }
        }

        public void WaitForBody()
        {
            _bodyReceived.WaitOne();
        }

        public void WaitForHeader()
        {
            _headerReceived.WaitOne();
        }
    }
}
