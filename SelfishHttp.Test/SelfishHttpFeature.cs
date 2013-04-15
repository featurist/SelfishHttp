using NUnit.Framework;

namespace SelfishHttp.Test
{
    [TestFixture]
    public abstract class SelfishHttpFeature
    {
        protected Server _server;
        protected string BaseUrl;

        [SetUp]
        public void SetUp()
        {
            int port = 12345;
            _server = new Server(port);
            BaseUrl = string.Format("http://localhost:{0}/", port);
        }

        [TearDown]
        public void TearDown()
        {
            _server.Stop();
        }
    }
}