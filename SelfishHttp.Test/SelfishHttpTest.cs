using System.Net;
using System.Net.Http;
using System.Threading;
using NUnit.Framework;

namespace SelfishHttp.Test
{
    [TestFixture]
    public class SelfishHttpTest
    {
        private Server _server;

        [SetUp]
        public void SetUp()
        {
            _server = new Server();
            _server.Start(12345);
        }

        [TearDown]
        public void TearDown()
        {
            _server.Stop();
        }

        [Test]
        public void ShouldReturnCorrectResource()
        {
            _server.OnGet("/stuff").RespondWith("yes, this is stuff");

            var client = new HttpClient();
            var response = client.GetAsync("http://localhost:12345/stuff").Result.Content.ReadAsStringAsync().Result;
            Assert.That(response, Is.EqualTo("yes, this is stuff"));
        }

        [Test]
        public void ShouldRedirectToAnotherUrl()
        {
            _server.OnGet("/stuff").RedirectTo("/otherstuff");
            _server.OnGet("/otherstuff").RespondWith("yes, this is other stuff");

            var client = new HttpClient();
            var response = client.GetAsync("http://localhost:12345/stuff").Result.Content.ReadAsStringAsync().Result;
            Assert.That(response, Is.EqualTo("yes, this is other stuff"));
        }

        [Test]
        public void ShouldResultIn404WhenUrlNotFound()
        {
            var client = new HttpClient();
            var statusCode = client.GetAsync("http://localhost:12345/stuff").Result.StatusCode;
            Assert.That(statusCode, Is.EqualTo(HttpStatusCode.NotFound));
        }
    }
}
