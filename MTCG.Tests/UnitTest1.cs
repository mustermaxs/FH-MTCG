using NUnit.Framework;
using System;
using MTCG;
using System.Reflection;
using NUnit.Framework.Internal;
using Moq;
using System.Collections.Generic;

namespace UnitTests.MTCG;

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
    public void Test_ReplacesTokensWithRegexPatterns(string routeTemplate, string expectedRoutePattern)
    {
        string trimmedRouteTemplate = parser.TrimUrl(routeTemplate);
        string generatedRegex = this.parser?.ReplaceTokensWithRegexPatterns(trimmedRouteTemplate) ?? "";
        Assert.That(generatedRegex, Is.EqualTo(expectedRoutePattern),
                    $"Generated regex '{generatedRegex}' does not match expected pattern '{expectedRoutePattern}'.");
    }

    public void Test_ParserReturnsNamedParamsInUrl(string routeTemplate, string expectedRoutePattern, string key, string expectedValue)
    {
        string trimmedRouteTemplate = parser.TrimUrl(routeTemplate);
        string generatedRegex = this.parser?.ReplaceTokensWithRegexPatterns(trimmedRouteTemplate) ?? "";
        Assert.That(generatedRegex, Is.EqualTo(expectedRoutePattern),
                    $"Generated regex '{generatedRegex}' does not match expected pattern '{expectedRoutePattern}'.");
    }

}

[TestFixture]

public class Test_Request
{
    [TestCase(HTTPMethod.GET, "{'name': 'test'}", "/mtcg/test/route")]
    public void Test_RequestBuilderReturnsRequest(HTTPMethod method, string Payload, string Path)
    {
        var mockHttpHeader = new Mock<HttpHeader>();
        mockHttpHeader.SetupGet(header => header.Name).Returns("MockHeaderName");
        mockHttpHeader.SetupGet(header => header.Value).Returns("MockHeaderValue");
        HttpHeader actualHeader = mockHttpHeader.Object;

        var headers = new HttpHeader[] { actualHeader };
        var reqBuilder = new RequestBuilder();

        IRequest request = reqBuilder
        .WithHeaders(headers)
        .WithPayload(Payload)
        .WithRoute(Path)
        .Build();

        Assert.That(request.Headers == headers);
        Assert.That(request.Payload == Payload);
        Assert.That(request.RawUrl == Path);
    }
}


[TestFixture]
public class MTCG_RouteRegistry
{
    IEndpointMapper? routeRegistry;
    private IUrlParser parser = new UrlParser();

    [SetUp]
    public void SetUp()
    {

        routeRegistry = RouteRegistry.GetInstance(parser);
    }


    [TestCase("/mtcg/user/{userid:int}/", "mtcg/user/23", true, HTTPMethod.GET)]
    [TestCase("/mtcg/user/{username:alpha}/", "mtcg/user/maximilian", true, HTTPMethod.PUT)]
    [TestCase("/mtcg/security/{token:alphanum}/", "mtcg/security/testtoken12398sdfkj98", true, HTTPMethod.POST)]
    [TestCase("/mtcg/route/without/params/", "/mtcg/route/without/params/", true, HTTPMethod.DELETE)]
    public void RouteRegistry_FindsRegisteredRoute(string routeTemplate, string requestedUrl, bool foundRoute, HTTPMethod method)
    {
        if (parser != null && routeRegistry != null)
        {
            string pattern = parser.ReplaceTokensWithRegexPatterns(routeTemplate);
            string regexRoutePattern = parser.ReplaceTokensWithRegexPatterns(routeTemplate);
            MethodInfo controllerMethod = typeof(TestController).GetMethod("TestMethod")!;
            IEndpoint endpoint = new Endpoint(method, routeTemplate, regexRoutePattern, typeof(TestController), controllerMethod);

            routeRegistry.RegisterEndpoint(endpoint);

            var resEndpoint = routeRegistry.MapRequestToEndpoint(requestedUrl, method);

            Assert.IsTrue(resEndpoint != null, $"{routeRegistry.GetType().Name} wasn't able to map the requested route.\n" +
            $"Requested Url: {requestedUrl}\n" +
            $"Template:     {routeTemplate}\n" +
            $"Pattern:      {pattern}");
        }

    }


}

public class TestController : IController
{
    public TestController(IRequest request) : base(request) { }
    public void TestMethod()
    {

    }
}

[TestFixture]
public class SessionTests
{
    [Test]
    public void SessionManager_CreatesAndReturnsSessionBySessionId()
    {
        Session session;
        var mockUser = new Mock<User>();
        mockUser.Setup(m => m.Name).Returns("Max");
        mockUser.Setup(m => m.Bio).Returns("Das ist die Bio");

        var authToken = "mtcg-token-12345";
        var mockUserObj = mockUser.Object;

        SessionManager.CreateSessionForUser(authToken, mockUserObj);
        Assert.True(SessionManager.TryGetSessionWithAuthToken(authToken, out session), $"Failed to get session");
        Assert.That(session.User!.Name == "Max");
        Assert.That(session.User.Bio == "Das ist die Bio");
        Assert.That(session.AuthToken == authToken, $"SessionId is incorrect.");
    }

    [TestCase]
    public void Session_IsAbleToAccessMultipleSessionsStatically()
    {
        var mockUser1 = new Mock<User>();
        var mockUser2 = new Mock<User>();
        var mockUser3 = new Mock<User>();

        SessionManager.CreateSessionForUser("123", mockUser1.Object);
        SessionManager.CreateSessionForUser("234", mockUser2.Object);
        SessionManager.CreateSessionForUser("345", mockUser3.Object);
        Session s1;
        Session s2;
        Session s3;

        SessionManager.TryGetSessionWithAuthToken("123", out s1);
        SessionManager.TryGetSessionWithAuthToken("234", out s2);
        SessionManager.TryGetSessionWithAuthToken("345", out s3);

        Assert.That(s1.AuthToken == "123", $"Failed to get session from SessionManager.");
        Assert.That(s2.AuthToken == "234", $"Failed to get session from SessionManager.");
        Assert.That(s3.AuthToken == "345", $"Failed to get session from SessionManager.");
    }

    [TestCase]
    public void SessionManager_EndsSession()
    {
        var mockUser = new Mock<User>();
        var mockUserObj = mockUser.Object;

        SessionManager.CreateSessionForUser("123", mockUserObj);
        SessionManager.EndSession("123");

        Assert.IsFalse(SessionManager.TryGetSessionWithAuthToken("123", out _), $"Session was not ended.");
    }

    [TestCase]
    public void SessionManager_EndsSessionWithInvalidAuthToken()
    {
        var mockUser = new Mock<User>();
        var mockUserObj = mockUser.Object;

        SessionManager.CreateSessionForUser("123", mockUserObj);
        SessionManager.EndSession("invalid authToken");

        Assert.IsTrue(SessionManager.TryGetSessionWithAuthToken("123", out _), $"Session was not ended.");
    }

    // [TestCase("password123")]
    // public void PasswordValidator_ValidatesCorrectPassword()
    // {
    //     IValidator passwordValidator = new PasswordValidator();
        
    // }
}

[Ignore("Reason for skipping this test class")]
[TestFixture]
public class Test_UserController
{
    [TestCase]
    public void UserController_LogsInUser()
    {
        var mockRequest = new Mock<IRequest>();
        var mockUser = new Mock<User>();
        string username = "maxi";
        string password = "maxiking";
        
        var controller = new UserController(mockRequest.Object);
        IResponse response = controller.Login();

        Assert.That(response.StatusCode == 200, $"Failed to login user. {response.Description}");
    }
}

[TestFixture]
[Ignore("Need to make a few changes. Source code changed")]
public class ControllerTests
{
    private IRequest mockRequest;
    private IEndpointMapper mockRouteRegistry;
    private IRouteObtainer mockRouteObtainer;
    private string responsePayload;

    [SetUp]
    public void Setup()
    {
        this.responsePayload = $"{{\"Name\":\"Michael\",\"Bio\":\"Halloooo.\",\"Password\":\"mikey\",\"Image\":\"###\",\"Coins\":32,\"ID\":\"897cb65a-4381-4c14-afda-65ff2cd291a4\"}}";
        
        var mockRouteObtainer = new Mock<IRouteObtainer>();
        this.mockRouteObtainer = mockRouteObtainer.Object;

        var mockEndpoint = new Mock<IEndpoint>();
        mockEndpoint.Setup(m => m.ControllerType).Returns(typeof(UserController));
        mockEndpoint.Setup(m => m.ControllerMethod).Returns(typeof(UserController).GetMethod("GetUserById")!);
        mockEndpoint.Setup(m => m.EndpointPattern).Returns("^users/([a-zA-Z0-9-]+)$");

        mockEndpoint.Setup(m => m.UrlParams).Returns(new Dictionary<string, string> { { "userid", "897cb65a-4381-4c14-afda-65ff2cd291a4" } });

        var mockRequest = new Mock<IRequest>();
        mockRequest.Setup(m => m.HttpMethod).Returns(HTTPMethod.GET);
        mockRequest.Setup(m => m.SessionId).Returns("123");
        mockRequest.Setup(m => m.Payload).Returns(this.responsePayload);
        mockRequest.Setup(m => m.Endpoint).Returns(mockEndpoint.Object);
        mockRequest.Setup(m => m.RawUrl).Returns("/users/897cb65a-4381-4c14-afda-65ff2cd291a4");
        this.mockRequest = mockRequest.Object;

        var mockRouteRegistry = new Mock<IEndpointMapper>();
        mockRouteRegistry.Setup(m => m.MapRequestToEndpoint(ref this.mockRequest));
        this.mockRouteRegistry = mockRouteRegistry.Object;
    }




    [TestCase]
    public void HandleRequest_ReturnsResponseObject()
    {
        var router = new Router(this.mockRouteRegistry, this.mockRouteObtainer);

        IResponse response = router.HandleRequest(ref this.mockRequest);

        Assert.IsTrue(response.PayloadAsJson() == this.mockRequest.Payload, $"{response.PayloadAsJson()} != {mockRequest.Payload}");
    }
}

[Controller]
public class ReflectionRouteObtainerTest : IController
{
    public ReflectionRouteObtainerTest(IRequest request) : base(request) { }

    [Route("/test/route", HTTPMethod.GET, Role.USER)]
    public void TestMethod()
    {
    }
}



[TestFixture]
public class Test_ReflectionUtils
{
    public static IEnumerable<object[]> ConvertToTypeTestCases()
    {
        yield return new object[] { "897cb65a-4381-4c14-afda-65ff2cd291a4", typeof(Guid), new Guid("897cb65a-4381-4c14-afda-65ff2cd291a4") };
        // Add more test cases here
    }

    [TestCaseSource(nameof(ConvertToTypeTestCases))]
    [TestCase("23", typeof(int), (int)23)]
    [TestCase("abc", typeof(string), (string)"abc")]
    [TestCase("true", typeof(bool), (bool)true)]
    [TestCase("false", typeof(bool), (bool)false)]
    [TestCase("false", typeof(bool), (bool)false)]
    [TestCase("3.1415", typeof(float), (float)3.1415)]
    public void Test_ConvertStringToExpectedTypes(string provided, Type expectedType, dynamic expectedValue)
    {
        var res = ReflectionUtils.ConvertToType(provided, expectedType);

        Assert.That(res == expectedValue, $"Converting string to {expectedType} failed.\nProvided: {provided}\nExpected: {expectedValue}");
    }

    [TestCase]
    public void ReflectionRouteObtainer_GetsPermissionAttributes()
    {
        var attrHandler = new AttributeHandler(Assembly.GetExecutingAssembly());
        var routeObtainer = new ReflectionRouteObtainer(attrHandler);

        List<Endpoint> endpoints = routeObtainer.ObtainRoutes();

        if (endpoints.Count <= 0)
        {
            Assert.Fail($"Failed to get endpoints.");

            return;
        }

        Assert.That(endpoints[0].AccessLevel == Role.USER && endpoints[0].RouteTemplate == "/test/route", $"Failed to get permissions from route attributes.");

    }
}




//    [TestFixture]
//     public class RouterTests
//     {
//         private IRequest mockRequest;
//         private IEndpointMapper routeRegistryMock;
//         private IRouteObtainer routeObtainerMock;

//         [SetUp]
//         public void Setup()
//         {
//             var mockEndpoint = new Mock<IEndpoint>();
//             mockEndpoint.Setup(m => m.ControllerType).Returns(typeof(TestController));
//             mockEndpoint.Setup(m => m.ControllerMethod).Returns(typeof(TestController).GetMethod("TestMethod")!);

//             var mockRequest = new Mock<IRequest>();
//             mockRequest.Setup(m => m.HttpMethod).Returns(HTTPMethod.GET);
//             mockRequest.Setup(m => m.Endpoint).Returns(mockEndpoint.Object);
//             mockRequest.Setup(m => m.RawUrl).Returns("/api/users/1");
//             this.mockRequest = mockRequest.Object;

//             var mockRouteRegistry = new Mock<IEndpointMapper>();
//             mockRouteRegistry.Setup(m => m.IsRouteRegistered(ref mockReques));
//             this.routeRegistryMock = mockRouteRegistry.Object;

//             // Create your mock RouteObtainer
//             var mockRouteObtainer = new Mock<IRouteObtainer>();
//             mockRouteObtainer.Setup(/* Set up your expectations */);
//             this.routeObtainerMock = mockRouteObtainer.Object;
//         }

//         [Test]
//         public void Test_HandleRequest_ReturnsResponse()
//         {
//             // Arrange
//             routeRegistryMock.SetupMapRequestToEndpoint(/* Set up your expectations */);

//             // Create an instance of your Router
//             var router = new Router(routeRegistryMock, routeObtainerMock);

//             // Act
//             var response = router.HandleRequest(ref mockRequest);

//             // Assert
//             Assert.NotNull(response);
//             // Add more assertions based on expected behavior
//         }

//         [Test]
//         public void HandleRequest_DbTransactionFailureException_Returns500Response()
//         {
//             // Arrange
//             routeRegistryMock.SetupMapRequestToEndpoint(/* Set up your expectations */);
//             routeRegistryMock.Setup(controller => controller.MapArgumentsAndInvoke<IResponse, string>(/* Set up your expectations */))
//                 .Throws(new DbTransactionFailureException("Transaction failed."));

//             // Create an instance of your Router
//             var router = new Router(routeRegistryMock, routeObtainerMock);

//             // Act
//             var response = router.HandleRequest(ref mockRequest);

//             // Assert
//             Assert.NotNull(response);
//             Assert.AreEqual(500, response.StatusCode);
//             // Add more assertions based on expected behavior
//         }

//         // Add more tests for other exceptions and scenarios
//     }

// // [TestFixture]
// // public class RouteRegistryTests
// // {
// //     private IUrlParser urlParser;
// //     private IEndpointMapper routeRegistry;

// //     [SetUp]
// //     public void Setup()
// //     {
// //         var urlParser = new UrlParser();
// //         routeRegistry = RouteRegistry.GetInstance(urlParser);
// //     }

// //     [Test]
// //     public void RegisterEndpoint_Success()
// //     {
// //         string routePattern = "/api/test/{id:int}/";
// //         HTTPMethod httpMethod = HTTPMethod.GET;
// //         Type controllerType = typeof(TestController);
// //         MethodInfo controllerMethod = typeof(TestController).GetMethod("TestMethod")!;

// //         routeRegistry.RegisterEndpoint(routePattern, httpMethod, controllerType, controllerMethod);

// //         var registeredEndpoints = routeRegistry.RegisteredEndpoints[httpMethod];
// //         Assert.AreEqual(1, registeredEndpoints.Count);

// //         var endpoint = (Endpoint)registeredEndpoints[0];

// //         Assert.AreEqual(httpMethod, endpoint.HttpMethod);
// //         Assert.AreEqual(routePattern, endpoint.RouteTemplate);
// //         Assert.AreEqual(controllerType, endpoint.ControllerType);
// //         Assert.AreEqual(controllerMethod, endpoint.ControllerMethod);
// //     }

// //     [Test]
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
