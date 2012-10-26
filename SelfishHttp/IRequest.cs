using System.Collections.Generic;
using System.IO;
using System.Net;

namespace SelfishHttp
{
    public interface IRequest
    {
        string Url { get; }
        WebHeaderCollection Headers { get; }
        dynamic Body { get; }
    }
}