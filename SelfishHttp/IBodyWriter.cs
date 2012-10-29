using System.IO;

namespace SelfishHttp
{
    public interface IBodyWriter
    {
        void WriteBody(object o, Stream stream);
    }
}