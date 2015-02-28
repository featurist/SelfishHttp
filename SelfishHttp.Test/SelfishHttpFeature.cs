using System;
using NUnit.Framework;

namespace SelfishHttp.Test
{
    [TestFixture]
    public abstract class SelfishHttpFeature
    {
        protected Server _server;

        [SetUp]
        public void SetUp()
        {
            int port = 12345;
            _server = new Server(port);
        }

        public string Url(string path)
        {
            return new Uri(new Uri(_server.BaseUri), path).AbsoluteUri;
        }

        [TearDown]
        public void TearDown()
        {
            _server.Stop();
        }
    }
}