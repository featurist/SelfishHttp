using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SelfishHttp
{
    public class BodyWriters : IBodyWriter
    {
        private readonly List<TypeWriter> _bodyWriters = new List<TypeWriter>();

        public void RegisterBodyWriter<T>(Action<T, Stream> writeBody)
        {
            _bodyWriters.Insert(0, new TypeWriter { Type = typeof(T), Writer = (o, stream) => writeBody((T)o, stream) });
        }

        public class TypeWriter
        {
            public Type Type;
            public Action<object, Stream> Writer;
        }

        public static IBodyWriter DefaultBodyWriter()
        {
            var writer = new BodyWriters();
            writer.RegisterBodyWriter<Stream>((stream, outputStream) => stream.CopyTo(outputStream));
            writer.RegisterBodyWriter<string>((str, outputStream) =>
                                                  {
                                                      using (var streamWriter = new StreamWriter(outputStream))
                                                      {
                                                          streamWriter.Write(str);
                                                      }
                                                  });

            return writer;
        }

        public void WriteBody(object o, Stream stream)
        {
            var writeBody = FindWriterForType(o.GetType());

            if (writeBody != null)
            {
                writeBody(o, stream);
            } else
            {
                throw new ApplicationException(string.Format("could not convert body of type {0}", o.GetType()));
            }
        }

        private Action<object,Stream> FindWriterForType(Type type)
        {
            TypeWriter writer = _bodyWriters.FirstOrDefault(typeWriter => typeWriter.Type.IsAssignableFrom(type));
            return writer != null? writer.Writer: null;
        }
    }
}