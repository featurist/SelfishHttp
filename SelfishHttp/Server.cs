using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace SelfishHttp
{
    public class Server : IDisposable
    {
        private HttpListener _listener;
        private List<IHttpHandler> _handlers = new List<IHttpHandler>();

        public Server(int port)
        {
            BaseUrl = String.Format("http://localhost:{0}/", port);
            Start();
        }

        public string BaseUrl { get; private set; }

        public IHttpHandler OnGet(string path)
        {
            return AddHttpHandler("GET", path);
        }

        public IHttpHandler OnHead(string path)
        {
            return AddHttpHandler("HEAD", path);
        }

        public IHttpHandler OnPut(string path)
        {
            return AddHttpHandler("PUT", path);
        }

        public IHttpHandler OnPost(string path)
        {
            return AddHttpHandler("POST", path);
        }

        public IHttpHandler OnDelete(string path)
        {
            return AddHttpHandler("DELETE", path);
        }

        private IHttpHandler AddHttpHandler(string method, string path)
        {
            var httpHandler = new MethodPathHttpHandler {Method = method, Path = path};
            _handlers.Add(httpHandler);
            return httpHandler;
        }

        private void Start()
        {
            _listener = new HttpListener();
            _listener.Prefixes.Add(BaseUrl);
            _listener.Start();
            HandleNextRequest();
        }

        private void HandleNextRequest()
        {
            _listener.BeginGetContext(OnRequest, null);
        }

        public void Stop()
        {
            _listener.Stop();
        }

        private void OnRequest(IAsyncResult ar)
        {
            if (_listener.IsListening)
            {
                HandleNextRequest();
                var context = _listener.EndGetContext(ar);
                HttpListenerRequest req = context.Request;
                HttpListenerResponse res = context.Response;

                var handler = _handlers.FirstOrDefault(h => h.Matches(req));

                if (handler != null)
                {
                    try
                    {
                        handler.Handle(req, res);
                    }
                    catch (Exception)
                    {
                        res.StatusCode = 500;
                        res.Close();
                    }
                }
                else
                {
                    res.StatusCode = 404;
                    res.Close();
                }
            }
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
