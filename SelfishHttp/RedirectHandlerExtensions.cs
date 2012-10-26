namespace SelfishHttp
{
    public static class RedirectHandlerExtensions
    {
        public static IHttpHandler RedirectTo(this IHttpHandler handler, string path)
        {
            handler.Handlers.Add((context, next) => context.Response.Redirect(path));
            return handler;
        }
    }
}