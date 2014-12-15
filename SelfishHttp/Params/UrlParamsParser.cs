using System.Collections.Specialized;
using System.Net;
using System.Web;

namespace SelfishHttp.Params
{
    class UrlParamsParser : IParamsParser
    {
        public NameValueCollection ParseParams(HttpListenerRequest request)
        {
            return HttpUtility.ParseQueryString(request.Url.Query);
        }
    }
}