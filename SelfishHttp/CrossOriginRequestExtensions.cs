namespace SelfishHttp
{
    public static class CrossOriginRequestExtensions
    {
        public static T AllowCrossOriginRequests<T>(this T handler)
            where T : IHttpHandler
        {
            return handler.Respond((req, res, next) =>
            {
                res.Headers["Access-Control-Allow-Origin"] = "*";
                res.Headers["Access-Control-Allow-Headers"] = req.Headers["Access-Control-Request-Headers"];
                if (req.Method == "OPTIONS")
                {
                }
                else
                {
                    next();
                }
            });
        }
    }
}