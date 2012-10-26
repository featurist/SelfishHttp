using System;
using System.Collections.Generic;
using System.Net;

namespace SelfishHttp
{
    public class MethodPathHttpHandler : IHttpHandler
    {
        public string Method;
        public string Path;
        public IList<Action<HttpListenerContext, Action>> Handlers { get; set; }

        public MethodPathHttpHandler()
        {
            AuthenticationScheme = AuthenticationSchemes.Anonymous;
            Handlers = new List<Action<HttpListenerContext, Action>>();
        }

        public void Handle(HttpListenerContext context)
        {
            var handlerEnumerator = Handlers.GetEnumerator();
            Action handle = null;
            handle = () =>
                         {
                             if (handlerEnumerator.MoveNext())
                             {
                                 handlerEnumerator.Current(context, () => handle());
                             }
                         };

            handle();
            context.Response.Close();
        }

        public AuthenticationSchemes AuthenticationSchemeFor(HttpListenerRequest httpRequest)
        {
            return AuthenticationScheme;
        }

        public AuthenticationSchemes AuthenticationScheme { get; set; }

        public bool Matches(HttpListenerRequest request)
        {
            return request.HttpMethod == Method && request.RawUrl == Path;
        }
    }
}