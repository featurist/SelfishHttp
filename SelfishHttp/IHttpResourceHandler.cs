using System;
using System.Net;

namespace SelfishHttp
{
    public interface IHttpHandler
    {
        void AddHandler(Action<HttpListenerContext, Action> handler);
        void Handle(HttpListenerContext context, Action next);
        AuthenticationSchemes? AuthenticationScheme { get; set; }
        IServerConfiguration ServerConfiguration { get; }
    }

    public interface IHttpResourceHandler : IHttpHandler
    {
        bool HasParameterMatching { get; }
        bool Matches(HttpListenerRequest request);
        IHttpResourceHandler IgnorePathCase();
        IHttpResourceHandler IgnoreParameterCase();
    }
}