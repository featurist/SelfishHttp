using System;
using System.IO;

namespace SelfishHttp
{
    public static class StringHandlerExtensions
    {
        public static IHttpHandler RespondWith(this IHttpHandler handler, string respondWith)
        {
            handler.Handle = (req, res) =>
                                  {
                                      using (var writer = new StreamWriter(res.OutputStream))
                                      {
                                          writer.Write(respondWith);
                                      }
                                  };

            return handler;
        }

        public static IHttpHandler RespondWith(this IHttpHandler handler, Func<string, string> responseFromRequest)
        {
            handler.Handle = (req, res) =>
                                  {
                                      string requestBody;

                                      using (var reader = new StreamReader(req.InputStream))
                                      {
                                          requestBody = reader.ReadToEnd();
                                      }

                                      var responseBody = responseFromRequest(requestBody);

                                      using (var writer = new StreamWriter(res.OutputStream))
                                      {
                                          writer.Write(responseBody);
                                      }
                                  };

            return handler;
        }
    }
}