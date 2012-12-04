using System;
using System.Collections.Specialized;
using System.Net;

namespace SelfishHttp
{
    public static class ProxyHandlerExtensions
    {
        public static T ForwardTo<T>(this T handler, string url) where T : IHttpHandler
        {
            handler.AddHandler((context, next) =>
                                   {
                                       var request = (HttpWebRequest) WebRequest.Create(new Uri(new Uri(url), context.Request.Url.AbsolutePath));
                                       request.Method = context.Request.HttpMethod;
                                       CopyHeaders(context.Request.Headers, request);

                                       if (request.Method == "POST" || request.Method == "PUT")
                                       {
                                           context.Request.InputStream.CopyTo(request.GetRequestStream());
                                       }

                                       var response = request.GetResponse();
                                       foreach (var header in response.Headers.AllKeys)
                                       {
                                           context.Response.AddHeader(header, response.Headers[header]);
                                       }
                                       response.GetResponseStream().CopyTo(context.Response.OutputStream);
                                   });
            return handler;
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
                        request.Host = value;
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