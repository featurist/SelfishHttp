using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace SelfishHttp
{
    public class Server : IDisposable, IServerConfiguration
    {
        private HttpListener _listener;
        private readonly HttpHandler _anyRequestHandler;
        private readonly List<IHttpResourceHandler> _resourceHandlers = new List<IHttpResourceHandler>();
        public IBodyParser BodyParser { get; set; }
        public IBodyWriter BodyWriter { get; set; }
        public IParamsParser ParamsParser { get; set; }

        public Server(int port)
        {
            BodyParser = BodyParsers.DefaultBodyParser();
            BodyWriter = BodyWriters.DefaultBodyWriter();
            ParamsParser = new UrlParamsParser();
            _anyRequestHandler = new HttpHandler(this);
            BaseUrl = String.Format("http://*:{0}/", port);
            Start();
        }

        public string BaseUrl { get; private set; }

        public IHttpResourceHandler OnGet(string path)
        {
            return AddHttpHandler("GET", path);
        }

        public IHttpResourceHandler OnHead(string path)
        {
            return AddHttpHandler("HEAD", path);
        }

        public IHttpResourceHandler OnPut(string path)
        {
            return AddHttpHandler("PUT", path);
        }

        public IHttpResourceHandler OnPost(string path)
        {
            return AddHttpHandler("POST", path);
        }

        public IHttpResourceHandler OnDelete(string path)
        {
            return AddHttpHandler("DELETE", path);
        }

        public IHttpResourceHandler OnOptions(string path)
        {
            return AddHttpHandler("OPTIONS", path);
        }

        public IHttpHandler OnRequest()
        {
            return _anyRequestHandler;
        }

        private IHttpResourceHandler AddHttpHandler(string method, string path)
        {
            var httpHandler = new HttpResourceHandler(method, path, this);
            _resourceHandlers.Add(httpHandler);
            return httpHandler;
        }

        private void Start()
        {
            _listener = new HttpListener();
            _listener.Prefixes.Add(BaseUrl);
            _listener.AuthenticationSchemeSelectorDelegate = AuthenticationSchemeSelectorDelegate;
            _listener.Start();
            HandleNextRequest();
        }

        private AuthenticationSchemes AuthenticationSchemeSelectorDelegate(HttpListenerRequest httpRequest)
        {
            if (_anyRequestHandler.AuthenticationScheme.HasValue)
            {
                return _anyRequestHandler.AuthenticationScheme.Value;
            }

            var handler = _resourceHandlers.FirstOrDefault(h => h.Matches(httpRequest));
            if (handler != null && handler.AuthenticationScheme.HasValue)
            {
                return handler.AuthenticationScheme.Value;
            }
            return AuthenticationSchemes.Anonymous;
        }

        private void HandleNextRequest()
        {
            _listener.BeginGetContext(HandleRequest, null);
        }

        public void Stop()
        {
            _listener.Stop();
        }

        private void HandleRequest(IAsyncResult ar)
        {
            if (_listener.IsListening)
            {
                HandleNextRequest();
                var context = _listener.EndGetContext(ar);
                HttpListenerRequest req = context.Request;
                HttpListenerResponse res = context.Response;

                _anyRequestHandler.Handle(context, () =>
                {
                    var handler = _resourceHandlers.FirstOrDefault(h => h.Matches(req));

                    if (handler != null)
                    {
                        try
                        {
                            handler.Handle(context, res.Close);
                        }
                        catch (Exception ex)
                        {
                            res.StatusCode = 500;
                            using (var output = new StreamWriter(res.OutputStream))
                            {
                                output.Write(ex);
                            }
                            res.Close();
                        }
                    }
                    else
                    {
                        res.StatusCode = 404;
                        res.Close();
                    }
                });

                res.Close();
            }
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
