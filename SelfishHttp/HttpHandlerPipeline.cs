using System;
using System.Collections.Generic;
using System.Net;

namespace SelfishHttp
{
    public class HttpHandler : IHttpHandler
    {
        private readonly IList<Action<HttpListenerContext, Action>> _handlers;

        public HttpHandler(IServerConfiguration serverConfig)
        {
            _handlers = new List<Action<HttpListenerContext, Action>>();
            ServerConfiguration = serverConfig;
        }

        public AuthenticationSchemes? AuthenticationScheme { get; set; }

        public IServerConfiguration ServerConfiguration { get; }

        public void Handle(HttpListenerContext context, Action next)
        {
            var handlerEnumerator = _handlers.GetEnumerator();

            void Action()
            {
                if (handlerEnumerator.MoveNext())
                    handlerEnumerator.Current(context, Action);
                else
                    next();
            }

            Action();
        }

        public void AddHandler(Action<HttpListenerContext, Action> handler)
        {
            _handlers.Add(handler);
        }

        internal void Clear()
        {
            _handlers.Clear();
        }
    }
}