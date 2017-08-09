using System;
using System.IO;

namespace SelfishHttp
{
    public static class StringHandlerExtensions
    {
        public static T RespondWith<T>(this T handler, string respondWith)
            where T : IHttpHandler
        {
            handler.AddHandler((context, next) =>
            {
                next();
                using (var writer = new StreamWriter(context.Response.OutputStream))
                {
                    writer.Write(respondWith);
                }
            });
            return handler;
        }

        public static T RespondWith<T>(this T handler, Func<string, string> responseFromRequest)
            where T : IHttpHandler
        {
            handler.AddHandler((context, next) =>
            {
                string requestBody;
                using (var reader = new StreamReader(context.Request.InputStream))
                {
                    requestBody = reader.ReadToEnd();
                }
                var responseBody = responseFromRequest(requestBody);
                next();
                using (var writer = new StreamWriter(context.Response.OutputStream))
                {
                    writer.Write(responseBody);
                }
            });
            return handler;
        }
    }
}