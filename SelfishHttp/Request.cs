using System.Collections.Generic;
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
        }

        public string Url { get; private set; }
        public string Method { get; private set; }
        public WebHeaderCollection Headers { get; private set; }
        public NameValueCollection Params { get; private set; }

        public T BodyAs<T>()
        {
            return (T) _bodyParser.ParseBody<T>(_request.InputStream);
        }
    }
}