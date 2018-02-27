using System.Collections.Specialized;
using System.Net;

namespace SelfishHttp
{
    public class Request : IRequest
    {
        private readonly IBodyParser _bodyParser;
        private readonly HttpListenerRequest _request;

        public Request(IServerConfiguration serverConfig, HttpListenerRequest request)
        {
            _bodyParser = serverConfig.BodyParser;
            _request = request;
            Url = request.Url.ToString();
            Method = request.HttpMethod;
            Headers = (WebHeaderCollection) request.Headers;
            Params = serverConfig.ParamsParser.ParseParams(_request);
            Cookies = _request.Cookies;
        }

        public CookieCollection Cookies { get; }

        public string Url { get; }

        public string Method { get; }

        public WebHeaderCollection Headers { get; }

        public NameValueCollection Params { get; }

        public T BodyAs<T>()
        {
            return (T) _bodyParser.ParseBody<T>(_request.InputStream);
        }
    }
}