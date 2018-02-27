using System;

namespace SelfishHttp
{
    public static class RequestResponseHandlerExtensions
    {
        public static T Respond<T>(this T handler, Action<IRequest, IResponse> handleRequest)
            where T : IHttpHandler
        {
            handler.AddHandler((context, next) =>
            {
                var config = handler.ServerConfiguration;
                var request = new Request(config, context.Request);
                var response = new Response(config, context.Response);
                handleRequest(request, response);
                next();
            });
            return handler;
        }

        public static T Respond<T>(this T handler, Action<IRequest, IResponse, Action> handleRequest)
            where T : IHttpHandler
        {
            handler.AddHandler((context, next) =>
            {
                var config = handler.ServerConfiguration;
                var request = new Request(config, context.Request);
                var response = new Response(config, context.Response);
                handleRequest(request, response, next);
            });
            return handler;
        }
    }
}