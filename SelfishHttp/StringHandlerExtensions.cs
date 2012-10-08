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
    }
}