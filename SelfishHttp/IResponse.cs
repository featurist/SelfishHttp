using System.Net;

namespace SelfishHttp
{
    public interface IResponse
    {
        int StatusCode { get; set; }

        WebHeaderCollection Headers { get; }

        object Body { set; }
    }
}