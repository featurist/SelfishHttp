using System;
using System.Net;

namespace SelfishHttp
{
    public interface IHttpHandler
    {
        AuthenticationSchemes? AuthenticationScheme { get; set; }

        IServerConfiguration ServerConfiguration { get; }

        void AddHandler(Action<HttpListenerContext, Action> handler);

        void Handle(HttpListenerContext context, Action next);
    }

    public interface IHttpResourceHandler : IHttpHandler
    {
        bool HasParameterMatching { get; }

        bool Matches(HttpListenerRequest request);

        IHttpResourceHandler IgnorePathCase();

        IHttpResourceHandler IgnoreParameterCase();
    }
}