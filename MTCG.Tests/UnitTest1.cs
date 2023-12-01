using NUnit.Framework;
using System;
using MTCG;

namespace UnitTests.MTCG
{
    [TestFixture]
    public class MTCG_UrlParser
    {
        private IUrlParser parser = new UrlParser();

        [SetUp]
        public void Setup()
        {
        }

        [TestCase("/mtcg/user/{userid:int}/", "^mtcg/user/(?<userid>[0-9]+)$")]
        [TestCase("/mtcg/user/{userid:int}/{username:alpha}/", "^mtcg/user/(?<userid>[0-9]+)/(?<username>[a-zA-Z-]+)$")]
        [TestCase("/mtcg/user/{username:alpha}/", "^mtcg/user/(?<username>[a-zA-Z-]+)$")]
        [TestCase("/mtcg/security/{token:alphanum}/", "^mtcg/security/(?<token>[a-zA-Z0-9-]+)$")]
        [TestCase("/mtcg/route/without/params/", "^mtcg/route/without/params$")]
        [TestCase("/api/{controller:alpha}/test/{view:alpha}/user/{userid:int}", "^api/(?<controller>[a-zA-Z-]+)/test/(?<view>[a-zA-Z-]+)/user/(?<userid>[0-9]+)$")]
        public void UrlParser_ReplacesTokensWithRegexPatterns(string routeTemplate, string expectedRoutePattern)
        {
            string trimmedRouteTemplate = parser.TrimUrl(routeTemplate);
            string generatedRegex = this.parser?.ReplaceTokensWithRegexPatterns(trimmedRouteTemplate) ?? "";
            Assert.That(generatedRegex, Is.EqualTo(expectedRoutePattern),
                        $"Generated regex '{generatedRegex}' does not match expected pattern '{expectedRoutePattern}'.");
        }

        //TODO
        // public void UrlParser_FindsNamedParams(string routeTemplate, string expectedRoutePattern, string key, string expectedValue)
        // {
        //     string trimmedRouteTemplate = parser.CleanUrl(routeTemplate);
        //     string generatedRegex = this.parser?.ReplaceTokensWithRegexPatterns(trimmedRouteTemplate) ?? "";
        //     Assert.That(generatedRegex, Is.EqualTo(expectedRoutePattern),
        //                 $"Generated regex '{generatedRegex}' does not match expected pattern '{expectedRoutePattern}'.");
        // }

    }

    [TestFixture]
    public class MTCG_RouteRegistry
    {
        EndpointMapper? routeResolver;
        private IUrlParser parser = new UrlParser();
        private string registeredGETRoute = "/api/{controller:alpha}/test/{view:alpha}/user/{userid:int}";

        [SetUp]
        public void SetUp()
        {
            routeResolver = new EndpointMapper(parser);
        }



        // [TestCase("/mtcg/user/{userid:int}/", "^mtcg/user/(?<userid>[0-9]+)", true, HTTPMethod.GET)]
        // [TestCase("/mtcg/user/{username:alpha}/", "^mtcg/user/(?<username>[a-zA-Z-]+)", true, HTTPMethod.PUT)]
        // [TestCase("/mtcg/security/{token:alphanum}/", "^mtcg/security/(?<token>[a-zA-Z0-9-]+)", true, HTTPMethod.POST)]
        // [TestCase("/mtcg/route/without/params/", "/mtcg/route/without/params/", true, HTTPMethod.DELETE)]


        [TestCase("/mtcg/user/{userid:int}/", "mtcg/user/23", true, HTTPMethod.GET)]
        [TestCase("/mtcg/user/{username:alpha}/", "mtcg/user/maximilian", true, HTTPMethod.PUT)]
        [TestCase("/mtcg/security/{token:alphanum}/", "mtcg/security/testtoken12398sdfkj98", true, HTTPMethod.POST)]
        [TestCase("/mtcg/route/without/params/", "/mtcg/route/without/params/", true, HTTPMethod.DELETE)]
        public void RouteRegistry_FindsRegisteredAndRequestedRoute(string routeTemplate, string requestedUrl, bool foundRoute, HTTPMethod method)
        {
            if (parser != null && routeResolver != null)
            {
                string pattern = parser.ReplaceTokensWithRegexPatterns(routeTemplate);

                try
                {
                    routeResolver.RegisterEndpoint(routeTemplate, method, typeof(IController), "Index");
                }
                catch (Exception ex)
                {
                    Assert.Fail($"Error: {ex}.");
                }

                RequestObject result = routeResolver.TryMapRouteToEndpoint(requestedUrl, method);

                Assert.IsTrue(result?.RouteFound, $"{routeResolver.GetType().Name} wasn't able to map the requested route.\n" +
                $"Requested Url: {requestedUrl}\n" +
                $"Template:     {routeTemplate}\n" +
                $"Pattern:      {pattern}");
            }
        }

        [TestCase("/mtcg/user/{userid:int}/", "mtcg/user/abc23", false, HTTPMethod.GET)]
        [TestCase("/mtcg/user/{username:alpha}/", "mtcg/wrong/maximilian234", false, HTTPMethod.PUT)]
        [TestCase("/mtcg/security/{token:alphanum}/", "mtcg/insecurity/testtoken12398sdfkj98", false, HTTPMethod.POST)]
        [TestCase("/mtcg/route/without/params/", "/mtcg/route/with/params/123/", false, HTTPMethod.DELETE)]
        public void RouteRegistry_CantFindRegisteredAndRequestedRoute(string routeTemplate, string requestedUrl, bool foundRoute, HTTPMethod method)
        {
            string pattern = parser.ReplaceTokensWithRegexPatterns(routeTemplate);
            routeResolver.RegisterEndpoint(routeTemplate, method, typeof(IController), "Index");
            RequestObject result = routeResolver.TryMapRouteToEndpoint(requestedUrl, method);
            Assert.IsFalse(result.RouteFound,
            $"---------------------------------------------------------------------\n" +
            $"{routeResolver.GetType().Name} should not have found the requested route.\n" +
            $"Url:      {requestedUrl}" +
            $"\nTemplate: {routeTemplate}" +
            $"\nPattern:  {pattern}");
        }
    }

    [TestFixture]
    public class MTCG_EndpointMapper
    {
        IUrlParser? parser;
        IEndpointMapper? mapper;
        [SetUp]
        public void SetUp()
        {
            this.parser = new UrlParser();
            this.mapper = new EndpointMapper(parser);
        }

        [TestCase("/mtcg/user/{userid:int}/", "mtcg/user/123", HTTPMethod.GET, true, "userid", 123)]
        public void EndpointMapper_ReturnsExpectedRequestObject(string urlTemplate, string url, HTTPMethod method, bool foundRoute, string paramKey, int value)
        {
            RequestObject request;
            mapper.RegisterEndpoint(urlTemplate, method, typeof(IController), "Index");
            request = mapper.TryMapRouteToEndpoint(url, method);
            Assert.That(request.RouteFound == true, $"Route not found");

            int intValue = request.GetParam<int>(paramKey);
            Assert.That(request.GetParam<int>(paramKey) == value, $"failed");
        }
    }

    // [TestFixture]
    // public class MTCG_HttpServer
    // {
    //     HttpServer? server;

    //     [SetUp]
    //     public void SetUp()
    //     {
    //         server = new HttpServer();
    //     }

    //     [Test]
    //     public void HttpServer_ServerStarts()
    //     {
    //         Console.WriteLine("start server");
    //         server.Run();
    //         Assert.IsTrue(server.Active, $"Server didn't start.");
    //         server.Stop();
    //     }

    //     [TearDown]
    //     public void HttpServer_StopServer()
    //     {
    //     }
    // }


    // [TestFixture]
    // public class MTCG_EndpointMapper
    // {
    //     IUrlParser parser;
    //     EndpointMapper mapper;
    //     [SetUp]
    //     public void SetUp()
    //     {
    //         parser = new UrlParser();
    //         mapper = new EndpointMapper(parser);
    //     }

    //     [Test]
    //     public void EndpointMapper_Returns
    // }
}