using System;
using System.Net;

namespace SelfishHttp
{
    public static class SecurityHandlerExtensions
    {
        public static T ProtectedWithBasicAuth<T>(this T handler, string username, string password)
            where T : IHttpHandler
        {
            return ProtectedWithBasicAuth(handler, (u, pw) => u == username && pw == password);
        }

        public static T ProtectedWithBasicAuth<T>(this T handler, Func<string, string, bool> areCredentialsCorrect)
            where T : IHttpHandler
        {
            handler.AuthenticationScheme = AuthenticationSchemes.Basic;
            handler.AddHandler((context, next) =>
            {
                if (context.Request.IsAuthenticated)
                {
                    var id = (HttpListenerBasicIdentity) context.User.Identity;
                    if (areCredentialsCorrect(id.Name, id.Password))
                        next();
                    else
                        context.Response.StatusCode = 403;
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