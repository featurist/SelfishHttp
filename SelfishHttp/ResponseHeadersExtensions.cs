namespace SelfishHttp
{
    public static class ResponseHeadersExtensions
    {
        public static T NoCache<T>(this T handler)
            where T : IHttpHandler
        {
            return handler.Respond((req, res, next) =>
            {
                res.Headers["Cache-Control"] = "no-cache";
                next();
            });
        }
    }
}