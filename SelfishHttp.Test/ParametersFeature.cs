using System.Net;
using System.Net.Http;

using NUnit.Framework;

namespace SelfishHttp.Test
{
    [TestFixture]
    public class ParametersFeature : SelfishHttpFeature
    {
        [Test]
        public void HandlerCanAccessUrlEncodedParameters()
        {
            _server.OnGet("/params").Respond((req, res) => { res.Body = req.Params["colour"]; });
            var client = new HttpClient();
            var response = client.GetAsync(Url("/params?colour=crimson")).Result;
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var body = response.Content.ReadAsStringAsync().Result;
            Assert.That(body, Is.EqualTo("crimson"));
        }
    }
}