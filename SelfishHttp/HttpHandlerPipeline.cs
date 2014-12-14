using System;
using System.Collections.Generic;
using System.Net;

namespace SelfishHttp
{
    public class HttpHandler : IHttpHandler
    {
        private readonly IList<Action<HttpListenerContext, Action>> _handlers;

        public AuthenticationSchemes? AuthenticationScheme { get; set; }
        public IServerConfiguration ServerConfiguration { get; private set; }

        public HttpHandler(IServerConfiguration serverConfig)
        {
            _handlers = new List<Action<HttpListenerContext, Action>>();
            ServerConfiguration = serverConfig;
        }

        internal void Clear()
        {
            _handlers.Clear();
        }

        public void Handle(HttpListenerContext context, Action next)
        {
            IEnumerator<Action<HttpListenerContext, Action>> handlerEnumerator = _handlers.GetEnumerator();
            Action handle = null;
            handle = () =>
            {
                if (handlerEnumerator.MoveNext())
                {
                    handlerEnumerator.Current(context, () => handle());
                }
                else
                {
                    next();
                }
            };

            handle();
        }

        public void AddHandler(Action<HttpListenerContext, Action> handler)
        {
            _handlers.Add(handler);
        }
    }
}