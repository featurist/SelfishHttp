using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using SelfishHttp.Params;
using SelfishHttp.Params.Matching;

namespace SelfishHttp
{
    public class Server : IDisposable, IServerConfiguration
    {
        private readonly string _uriPrefix;
        private readonly string _baseUri;
        private HttpListener _listener;
        private readonly HttpHandler _anyRequestHandler;
        private readonly object _locker = new object();
        private readonly Stack<IHttpResourceHandler> _resourceHandlers = new Stack<IHttpResourceHandler>();

        public IBodyParser BodyParser { get; set; }
        public IBodyWriter BodyWriter { get; set; }
        public IParamsParser ParamsParser { get; set; }

        public event EventHandler<RequestEventArgs> RequestBegin;

        public event EventHandler<RequestEventArgs> RequestNotFound;

        public event EventHandler<RequestErrorEventArgs> RequestError; 

        public Server()
            : this(ChooseRandomUnusedPort())
        {
        }

        public Server(int port)
        {
            _uriPrefix = String.Format("http://localhost:{0}/", port);
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

        public IHttpResourceHandler OnGet(string path, object parameters = null)
        {
            return AddHttpHandler("GET", path, parameters);
        }

        public IHttpResourceHandler OnHead(string path, object parameters = null)
        {
            return AddHttpHandler("HEAD", path, parameters);
        }

        public IHttpResourceHandler OnPut(string path, object parameters = null)
        {
            return AddHttpHandler("PUT", path, parameters);
        }

        public IHttpResourceHandler OnPatch(string path, object parameters = null)
        {
            return AddHttpHandler("PATCH", path, parameters);
        }

        public IHttpResourceHandler OnPost(string path, object parameters = null)
        {
            return AddHttpHandler("POST", path, parameters);
        }

        public IHttpResourceHandler OnDelete(string path, object parameters = null)
        {
            return AddHttpHandler("DELETE", path, parameters);
        }

        public IHttpResourceHandler OnOptions(string path, object parameters = null)
        {
            return AddHttpHandler("OPTIONS", path, parameters);
        }

        public IHttpHandler OnRequest()
        {
            return _anyRequestHandler;
        }

        private IHttpResourceHandler AddHttpHandler(string method, string path, object parameters)
        {
            IDictionary<string, IParamMatch> matches = null;

            if (parameters != null)
            {
                matches = parameters as IDictionary<string, IParamMatch>;

                if (matches != null)
                {
                    matches = matches.ToDictionary(kv => kv.Key.ToLowerInvariant(), kv => kv.Value);
                }
                else
                {
                    matches = new Dictionary<string, IParamMatch>();
                    var properties = TypeDescriptor.GetProperties(parameters);

                    foreach (PropertyDescriptor property in properties)
                    {
                        var propVal = property.GetValue(parameters);
                        var paramMatch = propVal as IParamMatch;
                        matches[property.Name.ToLowerInvariant()] = paramMatch ?? new StringMatch(Convert.ToString(propVal));
                    }
                }
            }

            var httpHandler = new HttpResourceHandler(method, path, matches, this);

            lock (_locker)
            {
                _resourceHandlers.Push(httpHandler);
            }

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

        /// <summary>
        ///     Clears this instance of any handlers registered by calls to <see cref="OnGet"/>, <see cref="OnHead"/>, 
        ///     <see cref="OnPut"/>, <see cref="OnPatch"/>, <see cref="OnPost"/>, <see cref="OnDelete"/> or <see cref="OnOptions"/>.
        ///     In addition, any handlers registered to the <see cref="OnRequest"/> <see cref="HttpHandler"/> instance are cleared.
        /// </summary>
        public void Clear()
        {
            lock (_locker)
            {
                _resourceHandlers.Clear();
                _anyRequestHandler.Clear();
            }
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
                        OnRequestBegin(new RequestEventArgs {Request = req, Response = res});

                        _anyRequestHandler.Handle(context, () =>
                        {
                            IHttpResourceHandler handler;
                            
                            lock(_locker)
                            {
                                handler = _resourceHandlers.Where(h => h.HasParameterMatching).FirstOrDefault(h => h.Matches(req)) ??
                                          _resourceHandlers.Where(h => !h.HasParameterMatching).FirstOrDefault(h => h.Matches(req));
                            }

                            if (handler != null)
                            {
                                handler.Handle(context, () => { });
                            }
                            else
                            {
                                res.StatusCode = 404;
                                OnRequestNotFound(new RequestEventArgs {Request = req, Response = res});
                            }
                        });

                        res.Close();
                    }
                    catch (Exception ex)
                    {
                        res.StatusCode = 500;
                        OnRequestError(new RequestErrorEventArgs { Exception = ex, Request = req, Response = res });
                            
                        Console.WriteLine(ex);
                        using (var output = new StreamWriter(res.OutputStream))
                        {
                            output.Write(ex);
                        }
                        res.Close();
                    }
                }
            }
            catch (HttpListenerException e)
            {
                if (!IsOperationAbortedOnStoppingServer(e))
                {
                    throw;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        /// <summary>
        /// Return true if the exception is: The I/O operation has been aborted because of either a thread exit or an application request.
        /// Happens when we stop the server and the listening is cancelled.
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private static bool IsOperationAbortedOnStoppingServer(HttpListenerException e)
        {
            return e.NativeErrorCode == 0x000003E3;
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

        private void OnRequestNotFound(RequestEventArgs e)
        {
            var handler = RequestNotFound;
            if (handler != null) handler(this, e);
        }

        private void OnRequestError(RequestErrorEventArgs e)
        {
            var handler = RequestError;
            if (handler != null) handler(this, e);
        }

        private void OnRequestBegin(RequestEventArgs e)
        {
            var handler = RequestBegin;
            if (handler != null) handler(this, e);
        }
    }
}