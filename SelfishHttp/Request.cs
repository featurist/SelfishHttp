using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;

namespace SelfishHttp
{
    public class Request : IRequest
    {
        public Request(HttpListenerRequest req)
        {
            Url = req.Url.ToString();
            Method = req.HttpMethod;
            Headers = (WebHeaderCollection) req.Headers;
            Body = new DynamicBody(req.InputStream);
        }

        public string Url { get; private set; }
        public string Method { get; private set; }
        public WebHeaderCollection Headers { get; private set; }
        public dynamic Body { get; private set; }
    }

    public class DynamicBody : DynamicObject
    {
        private readonly Stream _inputStream;

        public DynamicBody(Stream inputStream)
        {
            _inputStream = inputStream;
        }

        public override bool TryConvert(ConvertBinder binder, out object result)
        {
            if (binder.Type.IsAssignableFrom(typeof(Stream)))
            {
                result = _inputStream;
                return true;
            } else if (binder.Type.IsAssignableFrom(typeof(string)))
            {
                using (var reader = new StreamReader(_inputStream))
                {
                    result = reader.ReadToEnd();
                    return true;
                }
            } else
            {
                result = null;
                return false;
            }
        }
    }
}