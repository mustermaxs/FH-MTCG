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

//////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////


[TestFixture]
public class MTCG_RouteRegistry
{
    RouteRegistry? routeRegistry;
    private IUrlParser parser = new UrlParser();

    [SetUp]
    public void SetUp()
    {
        routeRegistry = (RouteRegistry)RouteRegistry.GetInstance(parser);
    }

    [TearDown]
    public void TearDown()
    {
        routeRegistry!.Dispose();
    }



    [TestCase("/mtcg/user/{userid:int}/", "mtcg/user/23", true, HTTPMethod.GET)]
    [TestCase("/mtcg/user/{username:alpha}/", "mtcg/user/maximilian", true, HTTPMethod.PUT)]
    [TestCase("/mtcg/security/{token:alphanum}/", "mtcg/security/testtoken12398sdfkj98", true, HTTPMethod.POST)]
    [TestCase("/mtcg/route/without/params/", "/mtcg/route/without/params/", true, HTTPMethod.DELETE)]
    public void RouteRegistry_FindsRegisteredRoute(string routeTemplate, string requestedUrl, bool foundRoute, HTTPMethod method)
    {
        if (parser != null && routeRegistry != null)
        {
            // routeTemplate = parser.TrimUrl(routeTemplate);
            // requestedUrl = parser.TrimUrl(requestedUrl);

            // string pattern = parser.ReplaceTokensWithRegexPatterns(routeTemplate);
            // string regexRoutePattern = parser.ReplaceTokensWithRegexPatterns(routeTemplate);
            // MethodInfo controllerMethod = typeof(TestController).GetMethod("TestMethod")!;
            // IEndpoint endpoint = new Endpoint(method, routeTemplate, regexRoutePattern, typeof(TestController), controllerMethod, Role.ANONYMOUS);
            var endpoint = new Mock<IEndpoint>();
            endpoint.Setup(e => e.HttpMethod).Returns(method);
            endpoint.Setup(e => e.RouteTemplate).Returns(routeTemplate);
            endpoint.Setup(e => e.EndpointPattern).Returns(parser.ReplaceTokensWithRegexPatterns(routeTemplate));
            endpoint.Setup(e => e.ControllerType).Returns(typeof(TestController));
            endpoint.Setup(e => e.ControllerMethod).Returns(typeof(TestController).GetMethod("TestMethod")!);
            endpoint.Setup(e => e.AccessLevel).Returns(Role.ANONYMOUS);

            routeRegistry.RegisterEndpoint(endpoint.Object);

            var resEndpoint = routeRegistry.MapRequestToEndpoint(requestedUrl, method);

            Assert.IsTrue(resEndpoint != null, $"{routeRegistry.GetType().Name} wasn't able to map the requested route.\n" +
            $"Requested Url: {requestedUrl}\n" +
            $"Template:     {routeTemplate}\n" +
            $"Pattern:      {endpoint.Object.EndpointPattern}");
        }

    }

    [TestCase("/cards/{id:alphanum}", "/cards/all", "/cards/all")]
    public void PrioritizesExactMatchOverRegex(string template1, string template2, string requested)
    {
        var endpoint = new Mock<IEndpoint>();
        var p1 = this.parser.ReplaceTokensWithRegexPatterns(template1);
        endpoint.Setup(e => e.HttpMethod).Returns(HTTPMethod.GET);
        endpoint.Setup(e => e.RouteTemplate).Returns(template1);
        endpoint.Setup(e => e.EndpointPattern).Returns(p1);
        endpoint.Setup(e => e.ControllerType).Returns(typeof(TestController));
        endpoint.Setup(e => e.ControllerMethod).Returns(typeof(TestController).GetMethod("TestMethod")!);
        endpoint.Setup(e => e.AccessLevel).Returns(Role.ANONYMOUS);
        var endpoint2 = new Mock<IEndpoint>();
        var p2 = this.parser.ReplaceTokensWithRegexPatterns(template2);
        endpoint2.Setup(e => e.HttpMethod).Returns(HTTPMethod.GET);
        endpoint2.Setup(e => e.RouteTemplate).Returns(template2);
        endpoint2.Setup(e => e.EndpointPattern).Returns(p2);
        endpoint2.Setup(e => e.ControllerType).Returns(typeof(TestController));
        endpoint2.Setup(e => e.ControllerMethod).Returns(typeof(TestController).GetMethod("TestMethod")!);
        endpoint2.Setup(e => e.AccessLevel).Returns(Role.ANONYMOUS);

        this.routeRegistry!.RegisterEndpoint(endpoint.Object);
        this.routeRegistry!.RegisterEndpoint(endpoint2.Object);

        var resEndpoint = routeRegistry!.MapRequestToEndpoint(requested, HTTPMethod.GET);


        Assert.That(resEndpoint != null && resEndpoint.RouteTemplate == requested,
        $"Detected {resEndpoint?.EndpointPattern} instead of {requested}");
    }
}

public class TestController : IController
{
    public TestController(IRequest request) : base(request) { }
    public void TestMethod()
    {

    }
}