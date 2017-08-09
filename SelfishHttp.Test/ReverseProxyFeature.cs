using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;

using NUnit.Framework;

namespace SelfishHttp.Test
{
    [TestFixture]
    public class ReverseProxyFeature
    {
        private Server _backendServer;
        private Server _proxyServer;

        [SetUp]
        public void SetUp()
        {
            _backendServer = new Server(12346);
            _proxyServer = new Server(12345);
        }

        [TearDown]
        public void TearDown()
        {
            _proxyServer.Stop();
            _backendServer.Stop();
        }

        [Test]
        public void ForwardsTheRequestButRewritesTheHostHeader()
        {
            _backendServer.OnGet("/stuff").Respond((req, res) => { res.Body = req.Headers["Host"]; });
            _proxyServer.OnRequest().ForwardTo("http://localhost:12346/");
            var client = new HttpClient();
            var response = client.GetAsync(_proxyServer.BaseUri + "stuff").Result;
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var body = response.Content.ReadAsStringAsync().Result;
            Assert.That(body, Is.EqualTo("localhost:12346"));
        }

        [Test]
        public void CanForwardHttpRequestToAnotherServer()
        {
            _backendServer.OnGet("/stuff").RespondWith("this is stuff!");
            _proxyServer.OnRequest().ForwardTo(_backendServer.BaseUri);
            var client = new HttpClient();
            var response = client.GetAsync(_proxyServer.BaseUri + "stuff").Result;
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var body = response.Content.ReadAsStringAsync().Result;
            Assert.That(body, Is.EqualTo("this is stuff!"));
        }

        [Test]
        public void ForwardsRequestRelativeToForwardUrl()
        {
            _backendServer.OnGet("/thingo/stuff").RespondWith("this is stuff!");
            _proxyServer.OnRequest().ForwardTo(_backendServer.BaseUri + "thingo/");
            var client = new HttpClient();
            var response = client.GetAsync(_proxyServer.BaseUri + "stuff").Result;
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var body = response.Content.ReadAsStringAsync().Result;
            Assert.That(body, Is.EqualTo("this is stuff!"));
        }

        [Test]
        public void ForwardsTheQueryString()
        {
            _backendServer.OnGet("/good").Respond((req, res) => res.Body = req.Params["foo"] + "," + req.Params["fruit"]);
            _proxyServer.OnRequest().ForwardTo(_backendServer.BaseUri);
            var client = new HttpClient();
            var response = client.GetAsync(_proxyServer.BaseUri + "good?foo=bar&fruit=banana").Result;
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Content.ReadAsStringAsync().Result, Is.EqualTo("bar,banana"));
        }

        [Test]
        public void CanForwardRequestWithBodyToAnotherServer()
        {
            _backendServer.OnPost("/stuff").Respond((req, res) => { res.Body = req.BodyAs<Stream>(); });
            _proxyServer.OnRequest().ForwardTo(_backendServer.BaseUri);
            var client = new HttpClient();
            var response = client.PostAsync(_proxyServer.BaseUri + "stuff", new StringContent("this is the body")).Result;
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var body = response.Content.ReadAsStringAsync().Result;
            Assert.That(body, Is.EqualTo("this is the body"));
        }

        [Test]
        public void CanForwardHeadersToAnotherServer()
        {
            _backendServer.OnGet("/stuff").Respond((req, res) => { res.Headers["X-Thing"] = req.Headers["X-Thing"]; });
            _proxyServer.OnRequest().ForwardTo(_backendServer.BaseUri);
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, _proxyServer.BaseUri + "stuff");
            request.Headers.Add("X-Thing", "the thing");
            var response = client.SendAsync(request).Result;
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Headers.GetValues("X-Thing").ElementAt(0), Is.EqualTo("the thing"));
        }

        [Test]
        public void ReturnsSameStatusCodeAsBackEnd()
        {
            _backendServer.OnGet("/stuff").Respond((req, res) =>
            {
                res.StatusCode = 400;
                res.Body = "this is the body";
            });
            _proxyServer.OnRequest().ForwardTo(_backendServer.BaseUri);
            var client = new HttpClient();
            var response = client.GetAsync(_proxyServer.BaseUri + "stuff").Result;
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(response.Content.ReadAsStringAsync().Result, Is.EqualTo("this is the body"));
        }
    }
}