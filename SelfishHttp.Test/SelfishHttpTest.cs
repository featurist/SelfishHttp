using System;
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
    }
}
