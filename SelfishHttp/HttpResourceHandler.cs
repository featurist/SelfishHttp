using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using SelfishHttp.Params.Matching;

namespace SelfishHttp
{
    public class HttpResourceHandler : IHttpResourceHandler
    {
        private string _method;
        private string _path;
        private readonly IDictionary<string, IParamsMatch> _paramsMatches;
        private HttpHandler _pipeline;
        private StringComparison _comparison;

        public IServerConfiguration ServerConfiguration { get; private set; }
        public AuthenticationSchemes? AuthenticationScheme { get; set; }

        public bool HasParameterMatching
        {
            get { return _paramsMatches != null; }
        }

        public HttpResourceHandler(string method, string path, IDictionary<string, IParamsMatch> paramsMatches, IServerConfiguration serverConfiguration)
        {
            _method = method;
            _path = path;
            _paramsMatches = paramsMatches;
            _pipeline = new HttpHandler(serverConfiguration);
            _comparison = StringComparison.CurrentCulture;
            ServerConfiguration = serverConfiguration;
            AuthenticationScheme = AuthenticationSchemes.Anonymous;
        }

        public void AddHandler(Action<HttpListenerContext, Action> handler)
        {
            _pipeline.AddHandler(handler);
        }

        public void Handle(HttpListenerContext context, Action next)
        {
            _pipeline.Handle(context, next);
        }

        public bool Matches(HttpListenerRequest request)
        {
            return request.HttpMethod == _method && string.Equals(request.Url.AbsolutePath, _path, _comparison) && MatchParameters(request);
        }

        private bool MatchParameters(HttpListenerRequest request)
        {
            if (_paramsMatches == null)
            {
                return true;
            }

            var parameters = ServerConfiguration.ParamsParser.ParseParams(request);
            var parameterKeys = parameters.Keys.Cast<string>().ToArray();

            if (!parameterKeys.Any())
            {
                return false;
            }

            var absentKeys = parameterKeys.Except(_paramsMatches.Keys).ToArray();

            if (absentKeys.Any() && !absentKeys.All(mk => _paramsMatches.ContainsKey(mk) && _paramsMatches[mk].IsOptional))
            {
                return false;
            }

            return _paramsMatches.Keys.Intersect(parameterKeys).All(k => _paramsMatches[k].IsMatch(parameters.GetValues(k)));
        }

        public IHttpResourceHandler IgnorePathCase()
        {
            _comparison = StringComparison.CurrentCultureIgnoreCase;
            return this;
        }
    }
}