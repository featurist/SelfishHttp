using System;
using System.Net;

namespace SelfishHttp
{
    public class HttpResourceHandler : IHttpResourceHandler
    {
        public IServerConfiguration ServerConfiguration { get; private set; }
        private string Method;
        private string Path;
        private HttpHandler _pipeline;
 
        public HttpResourceHandler(string method, string path, IServerConfiguration serverConfiguration)
        {
            Method = method;
            Path = path;
            ServerConfiguration = serverConfiguration;
            AuthenticationScheme = AuthenticationSchemes.Anonymous;
            _pipeline = new HttpHandler(serverConfiguration);
        }

        public void AddHandler(Action<HttpListenerContext, Action> handler)
        {
            _pipeline.AddHandler(handler);
        }

        public void Handle(HttpListenerContext context, Action next)
        {
            _pipeline.Handle(context, next);
        }

        public AuthenticationSchemes? AuthenticationScheme { get; set; }

        public bool Matches(HttpListenerRequest request)
        {
            return request.HttpMethod == Method && request.Url.AbsolutePath == Path;
        }
    }
}