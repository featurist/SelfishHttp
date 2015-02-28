using System.Text.RegularExpressions;
using System.Web;
using NUnit.Framework;
using SelfishHttp.Params.Matching;

namespace SelfishHttp.Test
{
    [TestFixture]
    public class ParametersMatchingFeature
    {
        [Test]
        public void ShouldMatchInt()
        {
            var parameters = HttpUtility.ParseQueryString("?id1=1&id2=0&id3=-1&id4=90");
            Assert.That(ParamIs.Int().IsMatch(parameters.GetValues("id1")), Is.True);
            Assert.That(ParamIs.Int().GreaterThanZero().IsMatch(parameters.GetValues("id1")), Is.True);
            Assert.That(ParamIs.Int().GreaterThanOrEqualToZero().IsMatch(parameters.GetValues("id2")), Is.True);
            Assert.That(ParamIs.Int().GreaterThanOrEqualToZero().IsMatch(parameters.GetValues("id1")), Is.True);
            Assert.That(ParamIs.Int().LessThanOrEqualToZero().IsMatch(parameters.GetValues("id2")), Is.True);
            Assert.That(ParamIs.Int().LessThanOrEqualToZero().IsMatch(parameters.GetValues("id3")), Is.True);
            Assert.That(ParamIs.Int().LessThanZero().IsMatch(parameters.GetValues("id3")), Is.True);
            Assert.That(ParamIs.Int().Between(89, 91).IsMatch(parameters.GetValues("id4")), Is.True);
        }

        [Test]
        public void ShouldMatchNotInt()
        {
            var parameters = HttpUtility.ParseQueryString("?id1=1&id2=0&id3=-1&id4=90&id5=HMYA");
            Assert.That(ParamIs.Int().IsMatch(parameters.GetValues("id5")), Is.False);
            Assert.That(ParamIs.Int().GreaterThanZero().IsMatch(parameters.GetValues("id2")), Is.False);
            Assert.That(ParamIs.Int().GreaterThanZero().IsMatch(parameters.GetValues("id3")), Is.False);
            Assert.That(ParamIs.Int().GreaterThanOrEqualToZero().IsMatch(parameters.GetValues("id3")), Is.False);
            Assert.That(ParamIs.Int().LessThanOrEqualToZero().IsMatch(parameters.GetValues("id1")), Is.False);
            Assert.That(ParamIs.Int().LessThanZero().IsMatch(parameters.GetValues("id2")), Is.False);
            Assert.That(ParamIs.Int().LessThanZero().IsMatch(parameters.GetValues("id1")), Is.False);
            Assert.That(ParamIs.Int().Between(90, 92).IsMatch(parameters.GetValues("id4")), Is.False);
            Assert.That(ParamIs.Int().Between(88, 90).IsMatch(parameters.GetValues("id4")), Is.False);
        }

        [Test]
        public void ShouldMatchEmpty()
        {
            var parameters = HttpUtility.ParseQueryString("?id=&id2=  ");
            Assert.That(ParamIs.Empty().IsMatch(parameters.GetValues("id")), Is.True);
            Assert.That(ParamIs.Empty().IsMatch(parameters.GetValues("id2")), Is.True);
        }

        [Test]
        public void ShouldMatchNotEmpty()
        {
            var parameters = HttpUtility.ParseQueryString("?id=ted");
            Assert.That(ParamIs.Empty().IsMatch(parameters.GetValues("id")), Is.False);
        }

        [Test]
        public void ShouldMatchAnything()
        {
            var parameters = HttpUtility.ParseQueryString("?id=a&id2=1");
            Assert.That(ParamIs.Anything().IsMatch(parameters.GetValues("id")), Is.True);
            Assert.That(ParamIs.Anything().IsMatch(parameters.GetValues("id2")), Is.True);
        }

        [Test]
        public void ShouldMatchNotAnything()
        {
            var parameters = HttpUtility.ParseQueryString("?id=");
            Assert.That(ParamIs.Anything().IsMatch(parameters.GetValues("id")), Is.False);
        }

        [Test]
        public void ShouldMatch()
        {
            var parameters = HttpUtility.ParseQueryString("?id=Ted");
            Assert.That(ParamIs.Equal("Ted").IsMatch(parameters.GetValues("id")), Is.True);
        }

        [Test]
        public void ShouldMatchIgnoreCase()
        {
            var parameters = HttpUtility.ParseQueryString("?id=Ted");
            Assert.That(ParamIs.Equal("ted").IgnoreCase().IsMatch(parameters.GetValues("id")), Is.True);
        }

        [Test]
        public void ShouldNotMatch()
        {
            var parameters = HttpUtility.ParseQueryString("?id=Ted");
            Assert.That(ParamIs.Equal("ted").IsMatch(parameters.GetValues("id")), Is.False);
        }

        [Test]
        public void ShouldNotMatchWithMany()
        {
            var parameters = HttpUtility.ParseQueryString("?id=1&id=2&id=3");
            Assert.That(new StringMatch("4").IsMatch(parameters.GetValues("id")), Is.False);
        }

        [Test]
        public void ShouldMatchAll()
        {
            var parameters = HttpUtility.ParseQueryString("?id=1");
            Assert.That(new AllOfMatch(new[] {"1"}).IsMatch(parameters.GetValues("id")), Is.True);
        }

        [Test]
        public void ShouldMatchAllWithMany()
        {
            var parameters = HttpUtility.ParseQueryString("?id=1&id=2&id=3");
            Assert.That(new AllOfMatch(new[] {"1", "2", "3"}).IsMatch(parameters.GetValues("id")), Is.True);
        }

        [Test]
        public void ShouldNotMatchAll()
        {
            var parameters = HttpUtility.ParseQueryString("?id=1&id=2");
            Assert.That(new AllOfMatch(new[] {"1"}).IsMatch(parameters.GetValues("id")), Is.False);

            parameters = HttpUtility.ParseQueryString("?id=1");
            Assert.That(new AllOfMatch(new[] {"1", "2"}).IsMatch(parameters.GetValues("id")), Is.False);
        }

        [Test]
        public void ShouldNotMatchAllWithMany()
        {
            var parameters = HttpUtility.ParseQueryString("?id=1&id=2&id=3&id=4");
            Assert.That(new AllOfMatch(new[] {"1", "2", "3"}).IsMatch(parameters.GetValues("id")), Is.False);

            parameters = HttpUtility.ParseQueryString("?id=1&id=2");
            Assert.That(new AllOfMatch(new[] {"1", "2", "3"}).IsMatch(parameters.GetValues("id")), Is.False);
        }

        [Test]
        public void ShouldMatchAny()
        {
            var parameters = HttpUtility.ParseQueryString("?id=1");
            Assert.That(new AnyOfMatch(new[] {"1", "2", "3"}).IsMatch(parameters.GetValues("id")), Is.True);
        }

        [Test]
        public void ShouldMatchAnyWithMany()
        {
            var parameters = HttpUtility.ParseQueryString("?id=1&id=2");
            Assert.That(new AnyOfMatch(new[] {"1", "2", "3", "4"}).IsMatch(parameters.GetValues("id")), Is.True);
        }

        [Test]
        public void ShouldNotMatchAny()
        {
            var parameters = HttpUtility.ParseQueryString("?id=1");
            Assert.That(new AnyOfMatch(new[] {"2"}).IsMatch(parameters.GetValues("id")), Is.False);

            parameters = HttpUtility.ParseQueryString("?id=1&id=2");
            Assert.That(new AnyOfMatch(new[] {"3"}).IsMatch(parameters.GetValues("id")), Is.False);
        }

        [Test]
        public void ShouldNotMatchAnyWithMany()
        {
            var parameters = HttpUtility.ParseQueryString("?id=1&id=2");
            Assert.That(new AnyOfMatch(new[] {"3", "4"}).IsMatch(parameters.GetValues("id")), Is.False);

            parameters = HttpUtility.ParseQueryString("?id=1&id=2&id=3");
            Assert.That(new AnyOfMatch(new[] {"3", "4"}).IsMatch(parameters.GetValues("id")), Is.False);
        }

        [Test]
        public void CanMatch()
        {
            var parameters = HttpUtility.ParseQueryString("?id=1");
            Assert.That(new RegexMatch(new Regex("[0-9]")).IsMatch(parameters.GetValues("id")), Is.True);
        }

        [Test]
        public void CanMatchWithMany()
        {
            var parameters = HttpUtility.ParseQueryString("?id=1&id=2&id=3");
            Assert.That(new RegexMatch(new Regex("[0-9]")).IsMatch(parameters.GetValues("id")), Is.True);
        }

        [Test]
        public void CannotMatch()
        {
            var parameters = HttpUtility.ParseQueryString("?id=1");
            Assert.That(new RegexMatch(new Regex("[^0-9]")).IsMatch(parameters.GetValues("id")), Is.False);
        }

        [Test]
        public void CannotMatchWithMany()
        {
            var parameters = HttpUtility.ParseQueryString("?id=1&id=2&id=3");
            Assert.That(new RegexMatch(new Regex("[^0-9]")).IsMatch(parameters.GetValues("id")), Is.False);
        }
    }
}