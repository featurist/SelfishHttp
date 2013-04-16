using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace SelfishHttp
{
    public class Server : IDisposable, IServerConfiguration
    {
        private readonly string _uriPrefix;
        private readonly string _baseUri;
        private HttpListener _listener;
        private readonly HttpHandler _anyRequestHandler;
        private readonly List<IHttpResourceHandler> _resourceHandlers = new List<IHttpResourceHandler>();

        public IBodyParser BodyParser { get; set; }
        public IBodyWriter BodyWriter { get; set; }
        public IParamsParser ParamsParser { get; set; }

        public Server()
            : this(ChooseRandomUnusedPort())
        {
        }

        public Server(int port)
        {
            _uriPrefix = String.Format("http://*:{0}/", port);
            _baseUri = string.Format("http://localhost:{0}/", port);
            BodyParser = BodyParsers.DefaultBodyParser();
            BodyWriter = BodyWriters.DefaultBodyWriter();
            ParamsParser = new UrlParamsParser();
            _anyRequestHandler = new HttpHandler(this);
            Start();
        }

        public string BaseUri
        {
            get { return _baseUri; }
        }

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

        public IHttpResourceHandler OnPatch(string path)
        {
            return AddHttpHandler("PATCH", path);
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
            _listener.Prefixes.Add(_uriPrefix);
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
            try
            {
                var context = _listener.EndGetContext(ar);
                if (_listener.IsListening)
                {
                    HandleNextRequest();
                    HttpListenerRequest req = context.Request;
                    HttpListenerResponse res = context.Response;

                    try
                    {
                        _anyRequestHandler.Handle(context, () =>
                        {
                            var handler = _resourceHandlers.FirstOrDefault(h => h.Matches(req));

                            if (handler != null)
                            {
                                handler.Handle(context, () => { });
                            }
                            else
                            {
                                res.StatusCode = 404;
                            }
                        });

                        res.Close();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                        res.StatusCode = 500;
                        using (var output = new StreamWriter(res.OutputStream))
                        {
                            output.Write(ex);
                        }
                        res.Close();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void Dispose()
        {
            Stop();
        }

        private static int ChooseRandomUnusedPort()
        {
            var listener = new TcpListener(IPAddress.Any, 0);
            listener.Start();
            var port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
        }
    }
}