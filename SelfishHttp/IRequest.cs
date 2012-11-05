using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;

namespace SelfishHttp
{
    public interface IRequest
    {
        string Url { get; }
        string Method { get; }
        WebHeaderCollection Headers { get; }
        NameValueCollection Params { get; }
        T BodyAs<T>();
    }
}