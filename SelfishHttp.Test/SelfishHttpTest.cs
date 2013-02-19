using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using NUnit.Framework;

namespace SelfishHttp.Test
{
    [TestFixture]
    public class SelfishHttpTest : SelfishHttpFeature
    {
        [Test]
        public void ShouldReturnCorrectResource()
        {
            _server.OnGet("/stuff").RespondWith("yes, this is stuff");

            var client = new HttpClient();
            var response = client.GetAsync("http://localhost:12345/stuff").Result.Content.ReadAsStringAsync().Result;
            Assert.That(response, Is.EqualTo("yes, this is stuff"));
        }

        [Test]
        public void ShouldHonourNoCacheBeforeRespondWith()
        {
            _server.OnGet("/stuff").NoCache().RespondWith("yes, this is stuff");

            var client = new HttpClient();
            var response = client.GetAsync("http://localhost:12345/stuff").Result;
            var responseHeaders = response.Headers;
            Assert.That(responseHeaders.CacheControl.NoCache, Is.True);
            Assert.That(response.Content.ReadAsStringAsync().Result, Is.EqualTo("yes, this is stuff"));
        }

        [Test]
        public void ShouldHonourNoCacheAfterRespondWith()
        {
            _server.OnGet("/stuff").RespondWith("yes, this is stuff").NoCache();

            var client = new HttpClient();
            var response = client.GetAsync("http://localhost:12345/stuff").Result;
            var responseHeaders = response.Headers;
            Assert.That(responseHeaders.CacheControl.NoCache, Is.True);
            Assert.That(response.Content.ReadAsStringAsync().Result, Is.EqualTo("yes, this is stuff"));
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
            _server.OnPut("/putsomethinghere").Respond((req, res) => { res.Body = req.BodyAs<Stream>(); });

            var client = new HttpClient();
            var response = client.PutAsync("http://localhost:12345/putsomethinghere", new StringContent("something to put")).Result.Content.ReadAsStringAsync().Result;
            Assert.That(response, Is.EqualTo("something to put"));
        }

        [Test]
        public void ShouldAcceptPatch()
        {
            _server.OnPatch("/sendmeapatch").Respond((req, res) => { res.StatusCode = 204; });

            var client = new HttpClient();
            var message = new HttpRequestMessage(new HttpMethod("PATCH"), "http://localhost:12345/sendmeapatch")
                {
                    Content = new StringContent("my patch")
                };
            var response = client.SendAsync(message).Result;
          
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
        }

        [Test]
        public void ShouldAcceptPost()
        {
            _server.OnPost("/postsomethinghere").Respond((req, res) => { res.Body = req.BodyAs<Stream>(); });

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
            _server.OnGet("/error").Respond((req, res) => { throw new Exception("bad stuff!"); });

            var client = new HttpClient();
            var response = client.GetAsync("http://localhost:12345/error").Result;
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));
            Assert.That(response.Content.ReadAsStringAsync().Result, Contains.Substring("bad stuff!"));
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

        [Test]
        public void RequestBodyCanBeAString()
        {
            string body = null;

            _server.OnPost("/post").Respond((req, res) =>
                                                {
                                                    body = req.BodyAs<string>();
                                                });

            var client = new HttpClient();

            var response = client.PostAsync("http://localhost:12345/post", new StringContent("hello")).Result;
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(body, Is.EqualTo("hello"));
        }

        [Test]
        public void ResponseBodyCanBeAString()
        {
            _server.OnGet("/get").Respond((req, res) =>
                                                {
                                                    res.Body = "hello";
                                                });

            var client = new HttpClient();

            var response = client.GetAsync("http://localhost:12345/get").Result;
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Content.ReadAsStringAsync().Result, Is.EqualTo("hello"));
        }

        [Test]
        public void ResponseBodyCanBeAStream()
        {
            _server.OnGet("/get").Respond((req, res) =>
                                                {
                                                    res.Body = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("hello"));
                                                });

            var client = new HttpClient();

            var response = client.GetAsync("http://localhost:12345/get").Result;
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Content.ReadAsStringAsync().Result, Is.EqualTo("hello"));
        }

        [Test]
        public void RequestBodyCanBeAStream()
        {
            string body = null;

            _server.OnPost("/post").Respond((req, res) =>
                                                {
                                                    var streamBody = req.BodyAs<Stream>();
                                                    using (var streamReader = new StreamReader(streamBody))
                                                    {
                                                        body = streamReader.ReadToEnd();
                                                    }
                                                });

            var client = new HttpClient();

            var response = client.PostAsync("http://localhost:12345/post", new StringContent("hello")).Result;
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(body, Is.EqualTo("hello"));
        }

        [Test]
        public void CanRespondToOptionsRequest()
        {
            _server.OnOptions("/options").Respond((req, res) =>
                                                      {
                                                          res.Body = "here are the options: ...";
                                                      });

            var client = new HttpClient();

            var request = new HttpRequestMessage(HttpMethod.Options, "http://localhost:12345/options");
            var response = client.SendAsync(request).Result;
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Content.ReadAsStringAsync().Result, Is.EqualTo("here are the options: ..."));
        }

        [Test]
        public void ResponsesCanBeDecorated()
        {
            _server.OnGet("/get").Respond((req, res, next) =>
            {
                res.Headers["x-custom"] = "thingo";
                next();
            }).Respond((req, res) =>
            {
                res.Body = "response";
            });

            var client = new HttpClient();

            var response = client.GetAsync(BaseUrl + "get").Result;
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Headers.GetValues("x-custom").Single(), Is.EqualTo("thingo"));
            Assert.That(response.Content.ReadAsStringAsync().Result, Is.EqualTo("response"));
        }

        [Test]
        public void AllServerResponsesCanBeDecorated()
        {
            _server.OnRequest().Respond((req, res) => { res.Headers["x-custom"] = "thingo"; });
            _server.OnGet("/get").RespondWith("hi");

            var client = new HttpClient();

            var response = client.GetAsync(BaseUrl + "get").Result;
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Headers.GetValues("x-custom").Single(), Is.EqualTo("thingo"));
            Assert.That(response.Content.ReadAsStringAsync().Result, Is.EqualTo("hi"));
        }

        [Test]
        public void Returns500IfOnRequestFails()
        {
            _server.OnRequest().Respond((req, res) => { throw new Exception("bad stuff!"); });

            var client = new HttpClient();

            var response = client.GetAsync(BaseUrl + "get").Result;
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));
        }
    }
}
