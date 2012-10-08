using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace SelfishHttp
{
    public class Server
    {
        private HttpListener _listener;
        private List<IHttpHandler> _handlers = new List<IHttpHandler>();

        public IHttpHandler OnGet(string path)
        {
            var httpHandler = new MethodPathHttpHandler { Method = "GET", Path = path };
            _handlers.Add(httpHandler);
            return httpHandler;
        }

        public void Start(int port)
        {
            _listener = new HttpListener();
            _listener.Prefixes.Add(String.Format("http://localhost:{0}/", port));
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
            HandleNextRequest();
            var context = _listener.EndGetContext(ar);
            HttpListenerRequest req = context.Request;
            HttpListenerResponse res = context.Response;

            var handler = _handlers.FirstOrDefault(h => h.Matches(req));

            if (handler != null)
            {
                handler.Respond(req, res);
            } else
            {
                res.StatusCode = 404;
                res.Close();
            }
        }
    }
}
