using System;

namespace SelfishHttp
{
    public class RequestErrorEventArgs : RequestEventArgs
    {
        public Exception Exception { get; internal set; }
    }
}