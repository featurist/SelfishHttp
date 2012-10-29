using System;
using System.IO;

namespace SelfishHttp
{
    public static class RequestResponseHandlerExtensions
    {
        public static IHttpHandler Respond(this IHttpHandler handler, Action<IRequest, IResponse> handleRequest)
        {
            handler.Handlers.Add((context, next) => handleRequest(new Request(handler.ServerConfiguration.BodyParser, context.Request), new Response(handler.ServerConfiguration.BodyWriter, context.Response)));

            return handler;
        }
    }
}