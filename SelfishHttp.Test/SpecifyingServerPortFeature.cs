using System;
using System.Net.Http;

using NUnit.Framework;

namespace SelfishHttp.Test
{
    [TestFixture]
    public class SpecifyingServerPortFeature
    {
        [Test]
        public void RandomPortNumberIsChosenWhenOmitted()
        {
            var firstServerWithoutPort = new Server();
            firstServerWithoutPort.OnGet("/").RespondWith("No problem");
            var secondServerWithoutPort = new Server();
            secondServerWithoutPort.OnGet("/").RespondWith("Still no problem");
            Assert.That(firstServerWithoutPort.BaseUri, Is.Not.EqualTo(secondServerWithoutPort.BaseUri));
            var client = new HttpClient();
            var firstResponse = client.GetAsync(firstServerWithoutPort.BaseUri).Result.Content.ReadAsStringAsync().Result;
            Assert.That(firstResponse, Is.EqualTo("No problem"));
            var secondResponse = client.GetAsync(secondServerWithoutPort.BaseUri).Result.Content.ReadAsStringAsync().Result;
            Assert.That(secondResponse, Is.EqualTo("Still no problem"));
        }

        [Test]
        public void SpecificPortNumberCanBeProvided()
        {
            var firstServerWithoutPort = new Server(8765);
            firstServerWithoutPort.OnGet("/").RespondWith("Rocking");
            Assert.That(new Uri(firstServerWithoutPort.BaseUri).Port, Is.EqualTo(8765));
            var response = new HttpClient().GetAsync(firstServerWithoutPort.BaseUri).Result.Content.ReadAsStringAsync().Result;
            Assert.That(response, Is.EqualTo("Rocking"));
        }
    }
}