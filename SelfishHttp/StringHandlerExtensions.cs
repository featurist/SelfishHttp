using System;
using System.IO;

namespace SelfishHttp
{
    public static class StringHandlerExtensions
    {
        public static IHttpHandler RespondWith(this IHttpHandler handler, string respondWith)
        {
            handler.Respond = (req, res) =>
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
            handler.Respond = (req, res) =>
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