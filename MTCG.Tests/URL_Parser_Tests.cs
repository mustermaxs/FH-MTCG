using NUnit.Framework;
using System;
using MTCG;
using System.Reflection;
using NUnit.Framework.Internal;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.IO;


namespace UnitTests.MTCG;

[TestFixture]
public class MTCG_UrlParser
{
    private IUrlParser parser = new UrlParser();

    [SetUp]
    public void Setup()
    {
    }

    [TestCase("/mtcg/user/{userid:int}/", "^mtcg/user/(?<userid>[0-9]+)")]
    [TestCase("/mtcg/user/{userid:int}/{username:alpha}/", "^mtcg/user/(?<userid>[0-9]+)/(?<username>[a-zA-Z-]+)")]
    [TestCase("/mtcg/user/{username:alpha}/", "^mtcg/user/(?<username>[a-zA-Z-]+)")]
    [TestCase("/mtcg/security/{token:alphanum}/", "^mtcg/security/(?<token>[a-zA-Z0-9-]+)")]
    [TestCase("/mtcg/route/without/params/", "^mtcg/route/without/params")]
    [TestCase("/api/{controller:alpha}/test/{view:alpha}/user/{userid:int}", "^api/(?<controller>[a-zA-Z-]+)/test/(?<view>[a-zA-Z-]+)/user/(?<userid>[0-9]+)")]
    public void Test_ReplacesTokensWithRegexPatterns(string routeTemplate, string expectedRoutePattern)
    {
        string trimmedRouteTemplate = parser.TrimUrl(routeTemplate);
        string generatedRegex = this.parser?.ReplaceTokensWithRegexPatterns(trimmedRouteTemplate) ?? "";
        Assert.That(generatedRegex, Is.EqualTo(expectedRoutePattern),
                    $"Generated regex '{generatedRegex}' does not match expected pattern '{expectedRoutePattern}'.");
    }

    [TestCase("/wrong/test", "/cards/all")]
    public void DoesntMatchIfPrefixWrong(string wrong, string requested)
    {
        var trimmedWrong = parser.TrimUrl(wrong);
        var timmedRequested = parser.TrimUrl(requested);
        string generatedRegex = this.parser?.ReplaceTokensWithRegexPatterns(trimmedWrong) ?? "";
        Assert.That(this.parser.PatternMatches(wrong, generatedRegex));
    }



    public void Test_ParserReturnsNamedParamsInUrl(string routeTemplate, string expectedRoutePattern, string key, string expectedValue)
    {
        string trimmedRouteTemplate = parser.TrimUrl(routeTemplate);
        string generatedRegex = this.parser?.ReplaceTokensWithRegexPatterns(trimmedRouteTemplate) ?? "";
        Assert.That(generatedRegex, Is.EqualTo(expectedRoutePattern),
                    $"Generated regex '{generatedRegex}' does not match expected pattern '{expectedRoutePattern}'.");
    }

    [TestCase("/test?filter=name&order=asc")]
    public void Test_ParserExtractsQueryParams(string requestedUrl)
    {
        string pattern = parser.ReplaceTokensWithRegexPatterns(requestedUrl);
        UrlParams urlParams = (UrlParams)parser.MatchUrlAndGetParams(requestedUrl, pattern);

        Assert.IsTrue(urlParams.QueryString["filter"] == "name");
        Assert.IsTrue(urlParams.QueryString["order"] == "asc");
    }

    [TestCase("/usersa", "/users")]
    [TestCase("/test/users", "/users")]
    [TestCase("/usersabcdef", "/users")]
    [TestCase("/users/123", "/users")]
    [TestCase("/usersabc?abc", "/users")]
    [TestCase("/test/usersabc?abc", "/users")]
    public void Test_DoesntMatchUrl(string wrongUrl, string correctUrl)
    {
        string pattern = parser.ReplaceTokensWithRegexPatterns(correctUrl);

        Assert.IsFalse(parser.PatternMatches(wrongUrl, pattern),
            $"Requested: {wrongUrl}\nPattern: {pattern}");
    }






    [Test]
    public void ExtractQueryParams_ReturnsCorrectDictionary()
    {
        var url = "https://mtcg.com/path?param1=value1&param2=value2&param3=value3";
        var urlPattern = "https://mtcg.com/path";

        var expectedParams = new Dictionary<string, string>
    {
        { "param1", "value1" },
        { "param2", "value2" },
        { "param3", "value3" }
    };

        var urlParser = new UrlParser();

        var actualParams = urlParser.ExtractQueryParams(url, urlPattern);

        Assert.AreEqual(expectedParams, actualParams);
    }




    [Test]
    public void Test_ReplaceTokensWithRegexPatterns()
    {
        var urlParser = new UrlParser();
        string url = "https://example.com/{username:alpha}/profile/{id:int}";

        string result = urlParser.ReplaceTokensWithRegexPatterns(url);

        Assert.AreEqual("^https://example.com/(?<username>[a-zA-Z-]+)/profile/(?<id>[0-9]+)", result);
    }



    [Test]
    public void ExtractNamedParams_ReturnsCorrectDictionary()
    {
        var url = "https://example.com/users/123";
        var urlPattern = @"^https:\/\/example\.com\/users\/(?<userId>\d+)$";
        var expectedParams = new Dictionary<string, string>
    {
        { "userId", "123" }
    };

        var urlParser = new UrlParser();

        var result = urlParser.ExtractNamedParams(url, urlPattern);

        Assert.AreEqual(expectedParams, result);
    }




    [Test]
    public void UrlParamsIndexer_ReturnsCorrectValue()
    {
        var urlParams = new UrlParams();
        urlParams["key1"] = "value1";
        urlParams["key2"] = "value2";

        string value1 = urlParams["key1"];
        string value2 = urlParams["key2"];
        string value3 = urlParams["key3"];

        Assert.AreEqual("value1", value1);
        Assert.AreEqual("value2", value2);
        Assert.AreEqual(string.Empty, value3);
    }
}
// //     public void RegisterEndpoint_DuplicateRoute()
// //     {
// //         string routePattern = "/api/test/{id:int}/";
// //         HTTPMethod httpMethod = HTTPMethod.GET;
// //         Type controllerType = typeof(TestController);
// //         MethodInfo controllerMethod = typeof(TestController).GetMethod("TestMethod")!;

// //         routeRegistry.RegisterEndpoint(routePattern, httpMethod, controllerType, controllerMethod);

// //         Assert.Throws<ArgumentException>(() =>
// //         {
// //             routeRegistry.RegisterEndpoint(routePattern, httpMethod, controllerType, controllerMethod);
// //         });
// //     }

// //     // Add more tests as needed
// // }



// // [TestCase("/mtcg/user/{userid:int}/", "mtcg/user/abc23", false, HTTPMethod.GET)]
// // [TestCase("/mtcg/user/{username:alpha}/", "mtcg/wrong/maximilian234", false, HTTPMethod.PUT)]
// // [TestCase("/mtcg/security/{token:alphanum}/", "mtcg/insecurity/testtoken12398sdfkj98", false, HTTPMethod.POST)]
// // [TestCase("/mtcg/route/without/params/", "/mtcg/route/with/params/123/", false, HTTPMethod.DELETE)]
// // public void RouteRegistry_CantFindRegisteredAndRequestedRoute(string routeTemplate, string requestedUrl, bool foundRoute, HTTPMethod method)
