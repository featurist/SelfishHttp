using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
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
            var response = client.GetAsync(Url("/stuff")).Result.Content.ReadAsStringAsync().Result;
            Assert.That(response, Is.EqualTo("yes, this is stuff"));
        }

        [Test]
        public void ShouldHonourIgnorePathCaseAndReturnCorrectResource()
        {
            _server.OnGet("/Stuff").IgnorePathCase().RespondWith("yes, this is stuff");

            var client = new HttpClient();
            var response = client.GetAsync(Url("/stuff")).Result.Content.ReadAsStringAsync().Result;
            Assert.That(response, Is.EqualTo("yes, this is stuff"));
        }

        [Test]
        public void ShouldReturnCorrectResourceWhenMatchingParameters()
        {
            _server.OnGet("/stuff").RespondWith("yes, tc1");
            _server.OnGet("/stuff", new { Id = "2" } ).RespondWith("yes, tc3");
            _server.OnGet("/stuff", new { Id = "(2+4)*10=60" }).RespondWith("yes, tc60");
            _server.OnGet("/stuff", new { Id = ParamIs.Equal("1")} ).RespondWith("yes, tc2");
            _server.OnGet("/stuff", new { Id = ParamIs.AllOf("1", "2") }).RespondWith("yes, tc4");
            _server.OnGet("/stuff", new { Id = ParamIs.AnyOf("3", "4", "5") }).RespondWith("yes, tc5");
            _server.OnGet("/stuff", new { Id = ParamIs.Like("^9[0-5]$") }).RespondWith("yes, tc6");
            _server.OnGet("/stuff", new { Id = ParamIs.Like(new Regex("^9[6-9]$")) }).RespondWith("yes, tc7");

            _server.OnGet("/stuff", new { Id = 901, MyKey = ParamIs.Equal("Yes").Optional() }).RespondWith("yes, tc8");
            _server.OnGet("/stuff", new { Id = 902, MyKey = ParamIs.Equal("Yes").Optional().IgnoreCase() }).RespondWith("yes, tc9");

            var client = new HttpClient();
            Assert.That(client.GetAsync(Url("/stuff")).Result.Content.ReadAsStringAsync().Result, Is.EqualTo("yes, tc1"));
            Assert.That(client.GetAsync(Url("/stuff?id=1")).Result.Content.ReadAsStringAsync().Result, Is.EqualTo("yes, tc2"));
            Assert.That(client.GetAsync(Url("/stuff?id=2")).Result.Content.ReadAsStringAsync().Result, Is.EqualTo("yes, tc3"));
            Assert.That(client.GetAsync(Url("/stuff?id=(2%2b4)*10%3d60")).Result.Content.ReadAsStringAsync().Result, Is.EqualTo("yes, tc60"));
            Assert.That(client.GetAsync(Url("/stuff?id=1&id=2")).Result.Content.ReadAsStringAsync().Result, Is.EqualTo("yes, tc4"));
            Assert.That(client.GetAsync(Url("/stuff?id=3&id=5")).Result.Content.ReadAsStringAsync().Result, Is.EqualTo("yes, tc5"));
            Assert.That(client.GetAsync(Url("/stuff?id=91")).Result.Content.ReadAsStringAsync().Result, Is.EqualTo("yes, tc6"));
            Assert.That(client.GetAsync(Url("/stuff?id=99")).Result.Content.ReadAsStringAsync().Result, Is.EqualTo("yes, tc7"));

            Assert.That(client.GetAsync(Url("/stuff?id=901")).Result.Content.ReadAsStringAsync().Result, Is.EqualTo("yes, tc8"));
            Assert.That(client.GetAsync(Url("/stuff?id=901&mykey=Yes")).Result.Content.ReadAsStringAsync().Result, Is.EqualTo("yes, tc8"));
            Assert.That(client.GetAsync(Url("/stuff?id=901&mykey=yes")).Result.Content.ReadAsStringAsync().Result, Is.EqualTo("yes, tc1"));

            Assert.That(client.GetAsync(Url("/stuff?id=902")).Result.Content.ReadAsStringAsync().Result, Is.EqualTo("yes, tc9"));
            Assert.That(client.GetAsync(Url("/stuff?id=902&mykey=Yes")).Result.Content.ReadAsStringAsync().Result, Is.EqualTo("yes, tc9"));
            Assert.That(client.GetAsync(Url("/stuff?id=902&mykey=yes")).Result.Content.ReadAsStringAsync().Result, Is.EqualTo("yes, tc9"));
        }

        [Test]
        public void ShouldHonourFifoRegistrationsWhenUsingParameterMatching()
        {
            var client = new HttpClient();

            _server.OnGet("/stuff", new { Id = ParamIs.Equal("2") }).RespondWith("yes, this is stuff");
            var responseFirst = client.GetAsync(Url("/stuff?id=2")).Result;

            _server.OnGet("/stuff", new { Id = "2" }).RespondWith("my mistake, this is stuff");
            var responseSecond = client.GetAsync(Url("/stuff?id=2")).Result;

            Assert.That(responseFirst.Content.ReadAsStringAsync().Result, Is.EqualTo("yes, this is stuff"));
            Assert.That(responseSecond.Content.ReadAsStringAsync().Result, Is.EqualTo("my mistake, this is stuff"));
        }

        [Test]
        public void ShouldHonourFifoRegistrations()
        {
            var client = new HttpClient();
            
            _server.OnGet("/stuff").RespondWith("yes, this is stuff");
            var responseFirst = client.GetAsync(Url("/stuff")).Result;

            _server.OnGet("/stuff").RespondWith("my mistake, this is stuff");
            var responseSecond = client.GetAsync(Url("/stuff")).Result;

            Assert.That(responseFirst.Content.ReadAsStringAsync().Result, Is.EqualTo("yes, this is stuff"));
            Assert.That(responseSecond.Content.ReadAsStringAsync().Result, Is.EqualTo("my mistake, this is stuff"));
        }

        [Test]
        public void ShouldHonourPathCase()
        {
            _server.OnGet("/Stuff").RespondWith("yes, this is stuff");

            var client = new HttpClient();
            var response = client.GetAsync(Url("/stuff")).Result;
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        }

        [Test]
        public void ShouldHonourNoCacheBeforeRespondWith()
        {
            _server.OnGet("/stuff").NoCache().RespondWith("yes, this is stuff");

            var client = new HttpClient();
            var response = client.GetAsync(Url("/stuff")).Result;
            var responseHeaders = response.Headers;
            Assert.That(responseHeaders.CacheControl.NoCache, Is.True);
            Assert.That(response.Content.ReadAsStringAsync().Result, Is.EqualTo("yes, this is stuff"));
        }

        [Test]
        public void ShouldHonourNoCacheAfterRespondWith()
        {
            _server.OnGet("/stuff").RespondWith("yes, this is stuff").NoCache();

            var client = new HttpClient();
            var response = client.GetAsync(Url("/stuff")).Result;
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
            var response = client.GetAsync(Url("/stuff")).Result.Content.ReadAsStringAsync().Result;
            Assert.That(response, Is.EqualTo("yes, this is other stuff"));
        }

        [Test]
        public void ShouldResultIn404WhenUrlNotFound()
        {
            var client = new HttpClient();
            var statusCode = client.GetAsync(Url("/stuff")).Result.StatusCode;
            Assert.That(statusCode, Is.EqualTo(HttpStatusCode.NotFound));
        }

        [Test]
        public void ShouldAcceptPut()
        {
            _server.OnPut("/putsomethinghere").Respond((req, res) => { res.Body = req.BodyAs<Stream>(); });

            var client = new HttpClient();
            var response = client.PutAsync(Url("/putsomethinghere"), new StringContent("something to put")).Result.Content.ReadAsStringAsync().Result;
            Assert.That(response, Is.EqualTo("something to put"));
        }

        [Test]
        public void ShouldAcceptPatch()
        {
            _server.OnPatch("/sendmeapatch").Respond((req, res) => { res.StatusCode = 204; });

            var client = new HttpClient();
            var message = new HttpRequestMessage(new HttpMethod("PATCH"), Url("/sendmeapatch"))
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
            var response = client.PostAsync(Url("/postsomethinghere"), new StringContent("something to post")).Result.Content.ReadAsStringAsync().Result;
            Assert.That(response, Is.EqualTo("something to post"));
        }

        [Test]
        public void ShouldAcceptDelete()
        {
            _server.OnDelete("/deletethis").RespondWith("deleted it");

            var client = new HttpClient();
            var response = client.DeleteAsync(Url("/deletethis")).Result.Content.ReadAsStringAsync().Result;
            Assert.That(response, Is.EqualTo("deleted it"));
        }

        [Test]
        public void ClearRemovesRegisteredHandlers()
        {
            _server.OnGet("/stuff").RespondWith("yes, this is stuff");
            _server.Clear();

            var client = new HttpClient();
            var response = client.GetAsync(Url("/stuff")).Result.StatusCode;

            Assert.That(response, Is.EqualTo(HttpStatusCode.NotFound));            
        }

        [Test]
        public void ClearRemovesAllRequestRegisteredHandlers()
        {
            var hit = false;
            
            _server.OnRequest().AddHandler((req, rep) => hit = true);
            _server.Clear();

            var client = new HttpClient();
            client.GetAsync(Url("/stuff")).Wait();
            Assert.That(hit, Is.False);
        }

        [Test]
        public void Returns500WhenResponseHandlerThrowsException()
        {
            _server.OnGet("/error").Respond((req, res) => { throw new Exception("bad stuff!"); });

            var client = new HttpClient();
            var response = client.GetAsync(Url("/error")).Result;
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

            var message = new HttpRequestMessage(HttpMethod.Get, Url("/arequest"));
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

            var response = client.GetAsync(Url("/arequest")).Result;
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Headers.GetValues("X-Custom").Single(), Is.EqualTo("hello there!"));
        }

        [Test]
        public void CanRespondToHeadRequest()
        {
            _server.OnHead("/head").Respond((req, res) => { res.Headers["X-Head-Received"] = req.Method == "HEAD"? "true": "false"; });

            var client = new HttpClient();

            var message = new HttpRequestMessage(HttpMethod.Head, Url("/head"));
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

            var response = client.PostAsync(Url("/post"), new StringContent("hello")).Result;
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

            var response = client.GetAsync(Url("/get")).Result;
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

            var response = client.GetAsync(Url("/get")).Result;
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

            var response = client.PostAsync(Url("/post"), new StringContent("hello")).Result;
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

            var response = client.GetAsync(Url("/get")).Result;
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

            var response = client.GetAsync(Url("/get")).Result;
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Headers.GetValues("x-custom").Single(), Is.EqualTo("thingo"));
            Assert.That(response.Content.ReadAsStringAsync().Result, Is.EqualTo("hi"));
        }

        [Test]
        public void Returns500IfOnRequestFails()
        {
            _server.OnRequest().Respond((req, res) => { throw new Exception("bad stuff!"); });

            var client = new HttpClient();

            var response = client.GetAsync(Url("/get")).Result;
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));
        }
    }
}
