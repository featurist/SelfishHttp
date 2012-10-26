using System;
using System.IO;

namespace SelfishHttp
{
    public static class StringHandlerExtensions
    {
        public static IHttpHandler RespondWith(this IHttpHandler handler, string respondWith)
        {
            handler.Handlers.Add((context, next) =>
                                  {
                                      using (var writer = new StreamWriter(context.Response.OutputStream))
                                      {
                                          writer.Write(respondWith);
                                      }
                                  });

            return handler;
        }

        public static IHttpHandler RespondWith(this IHttpHandler handler, Func<string, string> responseFromRequest)
        {
            handler.Handlers.Add((context, next) =>
                                  {
                                      string requestBody;

                                      using (var reader = new StreamReader(context.Request.InputStream))
                                      {
                                          requestBody = reader.ReadToEnd();
                                      }

                                      var responseBody = responseFromRequest(requestBody);

                                      using (var writer = new StreamWriter(context.Response.OutputStream))
                                      {
                                          writer.Write(responseBody);
                                      }
                                  });

            return handler;
        }
    }
}