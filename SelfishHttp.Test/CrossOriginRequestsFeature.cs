using System.Linq;
using System.Net;
using System.Net.Http;

using NUnit.Framework;

namespace SelfishHttp.Test
{
    [TestFixture]
    public class CrossOriginRequestsFeature : SelfishHttpFeature
    {
        [Test]
        public void AllowsCrossOriginRequests()
        {
            _server.OnRequest().AllowCrossOriginRequests();
            _server.OnGet("/cross-origin").RespondWith("hi");
            var client = new HttpClient();
            var url = Url("/cross-origin");
            var response = client.GetAsync(url).Result;
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Headers.GetValues("Access-Control-Allow-Origin").Single(), Is.EqualTo("*"));
            Assert.That(response.Content.ReadAsStringAsync().Result, Is.EqualTo("hi"));
            var request = new HttpRequestMessage(HttpMethod.Options, url);
            request.Headers.Add("Access-Control-Request-Headers", "origin, x-requested-with, accept");
            response = client.SendAsync(request).Result;
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Headers.GetValues("Access-Control-Allow-Origin").Single(), Is.EqualTo("*"));
            Assert.That(response.Headers.GetValues("Access-Control-Allow-Headers").Single(), Is.EqualTo("origin, x-requested-with, accept"));
        }
    }
}