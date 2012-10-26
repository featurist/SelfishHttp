using System;
using System.Net;

namespace SelfishHttp
{
    public class MethodPathHttpHandler : IHttpHandler
    {
        public string Method;
        public string Path;
        public Action<HttpListenerRequest, HttpListenerResponse> Handle { get; set; }

        public bool Matches(HttpListenerRequest request)
        {
            return request.HttpMethod == Method && request.RawUrl == Path;
        }
    }
}