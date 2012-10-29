using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
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
            Body = new DynamicBody(reqest.InputStream);
        }

        public string Url { get; private set; }
        public string Method { get; private set; }
        public WebHeaderCollection Headers { get; private set; }
        public dynamic Body { get; private set; }

        public T BodyAs<T>()
        {
            return (T) _bodyParsers.ParseBody<T>(_reqest.InputStream);
        }
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