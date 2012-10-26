using System;
using System.IO;

namespace SelfishHttp
{
    public static class RequestResponseHandlerExtensions
    {
        public static IHttpHandler Respond(this IHttpHandler handler, Action<IRequest, IResponse> handleRequest)
        {
            handler.Handlers.Add((context, next) => handleRequest(new Request(context.Request), new Response(context.Response)));

            return handler;
        }
    }
}