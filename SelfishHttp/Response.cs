using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;

namespace SelfishHttp
{
    public class Response : IResponse
    {
        private readonly HttpListenerResponse _response;

        public Response(HttpListenerResponse response)
        {
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

        public dynamic Body
        {
            set
            {
                if (value is string)
                {
                    using (var output = new StreamWriter(_response.OutputStream))
                    {
                        output.Write(value);
                    }
                }
            }
        }
    }
}