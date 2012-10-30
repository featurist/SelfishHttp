using System.Collections.Generic;
using System.IO;
using System.Net;

namespace SelfishHttp
{
    public interface IRequest
    {
        string Url { get; }
        string Method { get; }
        WebHeaderCollection Headers { get; }
        T BodyAs<T>();
    }
}