namespace SelfishHttp
{
    public static class RedirectHandlerExtensions
    {
        public static IHttpHandler RedirectTo(this IHttpHandler handler, string path)
        {
            handler.Respond = (req, res) =>
                                  {
                                      res.Redirect(path);
                                      res.Close();
                                  };
            return handler;
        }
    }
}