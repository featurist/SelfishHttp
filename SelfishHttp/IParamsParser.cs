using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;

namespace SelfishHttp
{
    public interface IParamsParser
    {
        NameValueCollection ParseParams(HttpListenerRequest request);
    }
}