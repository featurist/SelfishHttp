using System;
using System.IO;

namespace SelfishHttp
{
    public static class RequestResponseHandlerExtensions
    {
        public static IHttpHandler Respond(this IHttpHandler handler, Action<IRequest, IResponse> handleRequest)
        {
            handler.Handle = (req, res) =>
                                  {
                                      handleRequest(new Request(req), new Response(res));
                                      res.Close();
                                  };

            return handler;
        }
    }
}