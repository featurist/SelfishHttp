using System;
using System.Net;
using System.Runtime.InteropServices;

namespace SelfishHttp
{
    public class HttpResourceHandler : IHttpResourceHandler
    {
        public IServerConfiguration ServerConfiguration { get; private set; }
        private string Method;
        private string Path;
        private HttpHandler _pipeline;
        private StringComparison Comparison;
 
        public HttpResourceHandler(string method, string path, IServerConfiguration serverConfiguration)
        {
            Method = method;
            Path = path;
            ServerConfiguration = serverConfiguration;
            AuthenticationScheme = AuthenticationSchemes.Anonymous;
            _pipeline = new HttpHandler(serverConfiguration);
            Comparison = StringComparison.CurrentCulture;
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
            return request.HttpMethod == Method && string.Equals(request.Url.AbsolutePath, Path, Comparison);
        }

        public IHttpResourceHandler IgnorePathCase()
        {
            Comparison = StringComparison.CurrentCultureIgnoreCase;
            return this;
        }
    }
}