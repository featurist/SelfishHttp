using System;
using System.Net;

namespace SelfishHttp
{
    public class HttpResourceHandler : IHttpResourceHandler
    {
        private string _method;
        private string _path;
        private HttpHandler _pipeline;
        private StringComparison _comparison;

        public IServerConfiguration ServerConfiguration { get; private set; }
        public AuthenticationSchemes? AuthenticationScheme { get; set; }
 
        public HttpResourceHandler(string method, string path, IServerConfiguration serverConfiguration)
        {
            _method = method;
            _path = path;
            _pipeline = new HttpHandler(serverConfiguration);
            _comparison = StringComparison.CurrentCulture;
            ServerConfiguration = serverConfiguration;
            AuthenticationScheme = AuthenticationSchemes.Anonymous;
        }

        public void AddHandler(Action<HttpListenerContext, Action> handler)
        {
            _pipeline.AddHandler(handler);
        }

        public void Handle(HttpListenerContext context, Action next)
        {
            _pipeline.Handle(context, next);
        }

        public bool Matches(HttpListenerRequest request)
        {
            return request.HttpMethod == _method && string.Equals(request.Url.AbsolutePath, _path, _comparison);
        }

        public IHttpResourceHandler IgnorePathCase()
        {
            _comparison = StringComparison.CurrentCultureIgnoreCase;
            return this;
        }
    }
}