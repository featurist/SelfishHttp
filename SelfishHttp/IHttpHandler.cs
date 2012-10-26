using System;
using System.Collections.Generic;
using System.Net;

namespace SelfishHttp
{
    public interface IHttpHandler
    {
        bool Matches(HttpListenerRequest request);
        IList<Action<HttpListenerContext, Action>> Handlers { get; }
        AuthenticationSchemes AuthenticationScheme { get; set; }
        void Handle(HttpListenerContext context);
        AuthenticationSchemes AuthenticationSchemeFor(HttpListenerRequest httpRequest);
    }
}