using System;
using System.Collections.Generic;
using System.IO;

namespace SelfishHttp
{
    public class BodyParsers : IBodyParser
    {
        private readonly Dictionary<Type, Func<Stream, object>> _bodyParsers =
            new Dictionary<Type, Func<Stream, object>>();

        public object ParseBody<T>(Stream stream)
        {
            Func<Stream, object> parser;
            if (_bodyParsers.TryGetValue(typeof(T), out parser))
                return parser(stream);

            throw new ApplicationException(string.Format("could not convert body to type {0}", typeof(T)));
        }

        public void RegisterBodyParser<T>(Func<Stream, T> bodyParser)
        {
            _bodyParsers[typeof(T)] = stream => bodyParser(stream);
        }

        public static BodyParsers DefaultBodyParser()
        {
            var parsers = new BodyParsers();
            parsers.RegisterBodyParser(stream => stream);
            parsers.RegisterBodyParser(stream =>
            {
                using (var streamReader = new StreamReader(stream))
                {
                    return streamReader.ReadToEnd();
                }
            });
            return parsers;
        }
    }
}