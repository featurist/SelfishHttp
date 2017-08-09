namespace SelfishHttp
{
    public static class RedirectHandlerExtensions
    {
        public static T RedirectTo<T>(this T handler, string path)
            where T : IHttpHandler
        {
            handler.AddHandler((context, next) => context.Response.Redirect(path));
            return handler;
        }
    }
}