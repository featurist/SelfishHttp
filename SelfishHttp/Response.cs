using System.Net;

namespace SelfishHttp
{
    public class Response : IResponse
    {
        private readonly IBodyWriter _bodyWriter;
        private readonly HttpListenerResponse _response;

        public Response(IServerConfiguration config, HttpListenerResponse response)
        {
            _bodyWriter = config.BodyWriter;
            _response = response;
        }

        public int StatusCode
        {
            get => _response.StatusCode;
            set => _response.StatusCode = value;
        }

        public WebHeaderCollection Headers => _response.Headers;

        public object Body
        {
            set => _bodyWriter.WriteBody(value ?? "", _response.OutputStream);
        }
    }
}