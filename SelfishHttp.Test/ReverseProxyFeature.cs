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
        public void CanForwardHttpRequestToAnotherServer()
        {
            _backendServer.OnGet("/stuff").RespondWith("this is stuff!");
            _proxyServer.OnRequest().ForwardTo("http://localhost:12346/");

            var client = new HttpClient();
            var response = client.GetAsync("http://localhost:12345/stuff").Result;
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var body = response.Content.ReadAsStringAsync().Result;
            Assert.That(body, Is.EqualTo("this is stuff!"));
        }

        [Test]
        public void CanForwardRequestWithBodyToAnotherServer()
        {
            _backendServer.OnPost("/stuff").Respond((req, res) => { res.Body = req.BodyAs<Stream>(); });
            _proxyServer.OnRequest().ForwardTo("http://localhost:12346/");

            var client = new HttpClient();
            var response = client.PostAsync("http://localhost:12345/stuff", new StringContent("this is the body")).Result;
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var body = response.Content.ReadAsStringAsync().Result;
            Assert.That(body, Is.EqualTo("this is the body"));
        }

        [Test]
        public void CanForwardHeadersToAnotherServer()
        {
            _backendServer.OnGet("/stuff").Respond((req, res) => { res.Headers["X-Thing"] = req.Headers["X-Thing"]; });
            _proxyServer.OnRequest().ForwardTo("http://localhost:12346/");

            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost:12345/stuff");
            request.Headers.Add("X-Thing", "the thing");
            var response = client.SendAsync(request).Result;
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Headers.GetValues("X-Thing").ElementAt(0), Is.EqualTo("the thing"));
        }
    }
}