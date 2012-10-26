using System;
using System.Linq;
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
            _server = new Server(12345);
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

        [Test]
        public void ShouldAcceptPut()
        {
            _server.OnPut("/putsomethinghere").RespondWith(body => body);

            var client = new HttpClient();
            var response = client.PutAsync("http://localhost:12345/putsomethinghere", new StringContent("something to put")).Result.Content.ReadAsStringAsync().Result;
            Assert.That(response, Is.EqualTo("something to put"));
        }

        [Test]
        public void ShouldAcceptPost()
        {
            _server.OnPost("/postsomethinghere").RespondWith(body => body);

            var client = new HttpClient();
            var response = client.PostAsync("http://localhost:12345/postsomethinghere", new StringContent("something to post")).Result.Content.ReadAsStringAsync().Result;
            Assert.That(response, Is.EqualTo("something to post"));
        }

        [Test]
        public void ShouldAcceptDelete()
        {
            _server.OnDelete("/deletethis").RespondWith("deleted it");

            var client = new HttpClient();
            var response = client.DeleteAsync("http://localhost:12345/deletethis").Result.Content.ReadAsStringAsync().Result;
            Assert.That(response, Is.EqualTo("deleted it"));
        }

        [Test]
        public void Returns500WhenResponseHandlerThrowsException()
        {
            _server.OnGet("/error").RespondWith(body => { throw new Exception("bad stuff!"); });

            var client = new HttpClient();
            var response = client.GetAsync("http://localhost:12345/error").Result;
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));
        }

        [Test]
        public void CanInspectTheRequestHeaders()
        {
            _server.OnGet("/arequest").Respond((req, res) =>
                                                       {
                                                           res.Body = req.Headers["X-Custom"];
                                                       });

            var client = new HttpClient();

            var message = new HttpRequestMessage(HttpMethod.Get, "http://localhost:12345/arequest");
            message.Headers.Add("X-Custom", "hi!");
            var response = client.SendAsync(message).Result;
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Content.ReadAsStringAsync().Result, Is.EqualTo("hi!"));
        }

        [Test]
        public void CanSetResponseHeaders()
        {
            _server.OnGet("/arequest").Respond((req, res) =>
                                                       {
                                                           res.Headers["X-Custom"] = "hello there!";
                                                       });

            var client = new HttpClient();

            var response = client.GetAsync("http://localhost:12345/arequest").Result;
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Headers.GetValues("X-Custom").Single(), Is.EqualTo("hello there!"));
        }

        [Test]
        public void CanRespondToHeadRequest()
        {
            _server.OnHead("/head").Respond((req, res) => { res.Headers["X-Head-Received"] = req.Method == "HEAD"? "true": "false"; });

            var client = new HttpClient();

            var message = new HttpRequestMessage(HttpMethod.Head, "http://localhost:12345/head");
            var response = client.SendAsync(message).Result;
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Headers.GetValues("X-Head-Received").Single(), Is.EqualTo("true"));
        }
    }
}
