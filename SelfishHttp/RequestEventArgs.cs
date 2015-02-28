using System;
using System.Net;

namespace SelfishHttp
{
    public class RequestEventArgs : EventArgs
    {
        public HttpListenerRequest Request { get; internal set; }

        public HttpListenerResponse Response { get; internal set; }
    }
}