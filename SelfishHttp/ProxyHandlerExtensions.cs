using System;
using System.Collections.Specialized;
using System.Net;

namespace SelfishHttp
{
    public static class ProxyHandlerExtensions
    {
        public static T ForwardTo<T>(this T handler, string url)
            where T : IHttpHandler
        {
            handler.AddHandler((context, next) =>
            {
                var request = (HttpWebRequest) WebRequest.Create(new Uri(new Uri(url), context.Request.Url.PathAndQuery.TrimStart('/')));
                request.Method = context.Request.HttpMethod;
                CopyHeaders(context.Request.Headers, request);
                if (request.Method == "POST" || request.Method == "PUT")
                    context.Request.InputStream.CopyTo(request.GetRequestStream());
                var response = GetReponse(request);
                foreach (var header in response.Headers.AllKeys)

                    // content-length breaks copying the body stream
                    if (header.ToLower() != "content-length")
                        context.Response.AddHeader(header, response.Headers[header]);

                next();
                context.Response.StatusCode = (int) response.StatusCode;
                response.GetResponseStream().CopyTo(context.Response.OutputStream);
            });
            return handler;
        }

        private static HttpWebResponse GetReponse(HttpWebRequest request)
        {
            try
            {
                return (HttpWebResponse) request.GetResponse();
            }
            catch (WebException e)
            {
                if (e.Response != null)
                    return (HttpWebResponse) e.Response;

                throw;
            }
        }

        private static void CopyHeaders(NameValueCollection fromHeaders, HttpWebRequest request)
        {
            foreach (var header in fromHeaders.AllKeys)
            {
                var value = fromHeaders[header];
                switch (header.ToLower())
                {
                    case "accept":
                        request.Accept = value;
                        break;
                    case "connection":
                        if (value.ToLower() == "keep-alive")
                            request.KeepAlive = true;
                        else
                            request.Connection = value;
                        break;
                    case "content-length":
                        request.ContentLength = long.Parse(value);
                        break;
                    case "content-type":
                        request.ContentType = value;
                        break;
                    case "date":
                        request.Date = DateTime.Parse(value);
                        break;
                    case "expect":
                        break;
                    case "host":
                        break;
                    case "if-modified-since":
                        request.IfModifiedSince = DateTime.Parse(value);
                        break;
                    case "transfer-encoding":
                        request.TransferEncoding = value;
                        break;
                    case "user-agent":
                        request.UserAgent = value;
                        break;
                    case "referer":
                        request.Referer = value;
                        break;
                    default:
                        request.Headers.Add(header, fromHeaders[header]);
                        break;
                }
            }
        }
    }
}