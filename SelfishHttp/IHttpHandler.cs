using System;
using System.Net;

namespace SelfishHttp
{
    public interface IHttpHandler
    {
        bool Matches(HttpListenerRequest request);
        Action<HttpListenerRequest, HttpListenerResponse> Respond { get; set; }
    }
}