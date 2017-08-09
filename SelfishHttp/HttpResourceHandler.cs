using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;

using SelfishHttp.Params.Matching;

namespace SelfishHttp
{
    public class HttpResourceHandler : IHttpResourceHandler
    {
        private readonly string _method;
        private readonly IDictionary<string, IParamMatch> _paramsMatches;
        private readonly string _path;
        private readonly HttpHandler _pipeline;
        private StringComparer _comparer;
        private StringComparison _comparison;

        public HttpResourceHandler(string method, string path, IDictionary<string, IParamMatch> paramsMatches, IServerConfiguration serverConfiguration)
        {
            _method = method;
            _path = !path.EndsWith("/") ? path : path.Substring(0, path.Length - 1);
            _paramsMatches = paramsMatches;
            _pipeline = new HttpHandler(serverConfiguration);
            _comparison = StringComparison.CurrentCulture;
            _comparer = StringComparer.CurrentCulture;
            ServerConfiguration = serverConfiguration;
            AuthenticationScheme = AuthenticationSchemes.Anonymous;
        }

        public IServerConfiguration ServerConfiguration { get; }

        public AuthenticationSchemes? AuthenticationScheme { get; set; }

        public bool HasParameterMatching => _paramsMatches != null;

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
            var testPath = request.Url.AbsolutePath;
            return request.HttpMethod == _method &&
                   string.Equals(!testPath.EndsWith("/") ? testPath : testPath.Substring(0, testPath.Length - 1), _path, _comparison) &&
                   MatchParameters(request);
        }

        public IHttpResourceHandler IgnorePathCase()
        {
            _comparison = StringComparison.CurrentCultureIgnoreCase;
            return this;
        }

        public IHttpResourceHandler IgnoreParameterCase()
        {
            _comparer = StringComparer.CurrentCultureIgnoreCase;
            return this;
        }

        private bool MatchParameters(HttpListenerRequest request)
        {
            if (_paramsMatches == null)
                return true;

            var parameters = ServerConfiguration.ParamsParser.ParseParams(request);
            var parameterKeys = parameters.Keys.Cast<string>().Select(k => !k.Contains('[') ? k : k.Substring(0, k.IndexOf('['))).Distinct().ToArray();
            if (!parameterKeys.Any())
                return false;

            var absentKeys = parameterKeys.Except(_paramsMatches.Keys, _comparer).ToArray();
            if (absentKeys.Any() && !absentKeys.All(k => _paramsMatches.Any(kv => kv.Key.Equals(k, _comparison) && kv.Value.IsOptional)))
                return false;

            return _paramsMatches.Keys.Intersect(parameterKeys, _comparer)
                                 .All(k => _paramsMatches.First(kv => kv.Key.Equals(k, _comparison)).Value.IsMatch(GetValues(parameters, k)));
        }

        private string[] GetValues(NameValueCollection parameters, string key)
        {
            var keys = parameters.Keys.Cast<string>().ToArray();
            if (keys.Any(k => k.Equals(key)))
                return parameters.GetValues(key);

            return keys.Where(k => k.Length > key.Length + 1 && k.Substring(key.Length, 1).Equals("[")).SelectMany(parameters.GetValues).ToArray();
        }
    }
}