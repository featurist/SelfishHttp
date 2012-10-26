using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;

namespace SelfishHttp
{
    public interface IResponse
    {
        int StatusCode { get; set; }
        WebHeaderCollection Headers { get; }
        dynamic Body { set; }
    }
}