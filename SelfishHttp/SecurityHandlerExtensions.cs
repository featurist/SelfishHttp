using System;
using System.Net;

namespace SelfishHttp
{
    public static class SecurityHandlerExtensions
    {
        public static IHttpHandler ProtectedWithBasicAuth(this IHttpHandler handler, string username, string password)
        {
            return ProtectedWithBasicAuth(handler, (u, pw) => u == username && pw == password);
        }

        public static IHttpHandler ProtectedWithBasicAuth(this IHttpHandler handler, Func<string, string, bool> areCredentialsCorrect)
        {
            handler.AuthenticationScheme = AuthenticationSchemes.Basic;
            handler.Handlers.Add((context, next) =>
                                     {
                                         if (context.Request.IsAuthenticated)
                                         {
                                             var id = (HttpListenerBasicIdentity) context.User.Identity;
                                             if (areCredentialsCorrect(id.Name, id.Password))
                                             {
                                                 next();
                                             } else
                                             {
                                                 context.Response.StatusCode = 403;
                                             }
                                         }
                                         else
                                         {
                                             context.Response.StatusCode = 401;
                                         }
                                     });

            return handler;
        }
    }
}