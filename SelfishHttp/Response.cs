using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;

namespace SelfishHttp
{
    public class Response : IResponse
    {
        private readonly IBodyWriter _bodyWriter;
        private readonly HttpListenerResponse _response;

        public Response(IBodyWriter bodyWriter, HttpListenerResponse response)
        {
            _bodyWriter = bodyWriter;
            _response = response;
        }

        public int StatusCode
        {
            get { return _response.StatusCode; }
            set { _response.StatusCode = value; }
        }

        public WebHeaderCollection Headers
        {
            get { return _response.Headers; }
        }

        public object Body
        {
            set { _bodyWriter.WriteBody(value ?? "", _response.OutputStream); }
        }
    }
}