using System.IO;

namespace SelfishHttp
{
    public interface IBodyParser
    {
        object ParseBody<T>(Stream stream);
    }
}