using System;
using System.IO;
using System.Net;

using NUnit.Framework;

namespace SelfishHttp.Test
{
    [TestFixture]
    public class BasicAuthenticationFeature : SelfishHttpFeature
    {
        [Test]
        public void AllowsAuthencatedRequestsToProtectedResourcesWithCorrectCredentials()
        {
            _server.OnGet("/private").ProtectedWithBasicAuth("username", "password").RespondWith("this is private!");
            var response = RequestWithBasicAuth(Url("/private"), "username", "password");
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            using (var reader = new StreamReader(response.GetResponseStream()))
            {
                Assert.That(reader.ReadToEnd(), Is.EqualTo("this is private!"));
            }
        }

        [Test]
        public void DisallowsAuthencatedRequestsToProtectedResourcesWithIncorrectCredentials()
        {
            _server.OnGet("/private").ProtectedWithBasicAuth("username", "password").RespondWith("this is private!");
            CannotAccessWithInvalidCredentials(Url("/private"));
        }

        private static void CannotAccessWithInvalidCredentials(string url)
        {
            try
            {
                RequestWithBasicAuth(url, "username", "badpassword");
            }
            catch (WebException we)
            {
                Assert.That(we.Response, Is.Not.Null);
                var response = (HttpWebResponse) we.Response;
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    Assert.That(reader.ReadToEnd(), Is.EqualTo(""));
                }
            }
        }

        [Test]
        public void CanProtectAllResourcesOnServer()
        {
            _server.OnRequest().ProtectedWithBasicAuth("username", "password");
            _server.OnGet("/a").RespondWith("private a");
            _server.OnGet("/a").RespondWith("private b");
            CannotAccessWithInvalidCredentials(Url("/a"));
            CannotAccessWithInvalidCredentials(Url("/b"));
        }

        private static HttpWebResponse RequestWithBasicAuth(string requestUriString, string userName, string password)
        {
            var request = (HttpWebRequest) WebRequest.Create(requestUriString);
            var credentialCache = new CredentialCache();
            credentialCache.Add(new Uri(new Uri(requestUriString).GetLeftPart(UriPartial.Authority)), "Basic", new NetworkCredential(userName, password));
            request.Credentials = credentialCache;
            request.PreAuthenticate = true;
            var response = (HttpWebResponse) request.GetResponse();
            return response;
        }
    }
}