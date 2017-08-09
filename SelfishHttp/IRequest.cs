using System.Collections.Specialized;
using System.Net;

namespace SelfishHttp
{
    public interface IRequest
    {
        string Url { get; }

        string Method { get; }

        WebHeaderCollection Headers { get; }

        NameValueCollection Params { get; }

        CookieCollection Cookies { get; }

        T BodyAs<T>();
    }
}