using NUnit.Framework;
using MTCG; // Add the correct namespace for IUrlParser

namespace UnitTests.Routing
{
    [TestFixture]
    public class MTCG_UrlParser
    {
        private IUrlParser parser;
        private string requestedUrl;

        [SetUp]
        public void Setup()
        {
            parser = new UrlParser();
            requestedUrl = "/api/{controller:a}/{view:a}/test/";
        }

        [Test]
        public void UrlParser_GeneratesRegexPatternForAlphaNumericNamedGroup()
        {
            string regexPattern = parser.CreateRegexPattern(requestedUrl);
            StringAssert.IsMatch(@"^((api/(?<controller>[a-zA-Z-]+)/(?<view>[a-zA-Z-]+)))$", regexPattern);
        }
    }
}
