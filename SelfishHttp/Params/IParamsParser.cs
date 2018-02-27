using System.Collections.Specialized;
using System.Net;

namespace SelfishHttp.Params
{
    public interface IParamsParser
    {
        NameValueCollection ParseParams(HttpListenerRequest request);
    }
}