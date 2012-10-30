using System.Net;

namespace SelfishHttp
{
    public class Request : IRequest
    {
        private readonly IBodyParser _bodyParsers;
        private readonly HttpListenerRequest _reqest;

        public Request(IBodyParser bodyParsers, HttpListenerRequest reqest)
        {
            _bodyParsers = bodyParsers;
            _reqest = reqest;
            Url = reqest.Url.ToString();
            Method = reqest.HttpMethod;
            Headers = (WebHeaderCollection) reqest.Headers;
        }

        public string Url { get; private set; }
        public string Method { get; private set; }
        public WebHeaderCollection Headers { get; private set; }

        public T BodyAs<T>()
        {
            return (T) _bodyParsers.ParseBody<T>(_reqest.InputStream);
        }
    }
}