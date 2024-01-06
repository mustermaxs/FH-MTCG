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


//////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////


[TestFixture]
public class Test_ConfigService
{
    public ConfigService configService = ConfigService.GetInstance();
    [SetUp]
    public void Setup()
    {
        Directory.SetCurrentDirectory("/home/mustermax/vscode_projects/MTCG/MTCG.Tests/");
    }

    [Test]
    public void Register_WithValidPath_ShouldRegisterConfig()
    {
        string path = "config.json";

        configService.Register<MockConfig>(path);

        Assert.IsTrue(ConfigService.Get<MockConfig>() != null, "Failed to register config");
    }

    [Test]
    public void Register_WithInvalidPath_ShouldThrowException()
    {
        string path = "invalid_config.json";

        Assert.Throws<FileNotFoundException>(() => configService.Register<MockConfig>(path), "Failed to throw exception for invalid path");
    }

    [Test]
    public void GetsSubSections()
    {
        string section = "a/b/c";
        string[] res = { "a", "b", "c"};

        Assert.That(ConfigService.GetSubsectionKeys(section).Any(s => res.Contains(s)), "Failed to get subsections");
    }

    [Test]
    public void Gets_ValueFromNestedJsonObject()
    {
        string path = "config.json";
        configService.Register<NestedConfig>(path);

        var config = (NestedConfig)ConfigService.Get<NestedConfig>();

        Assert.That(config.AnswerIs == 42, "Config has wrong value for AnswerIs.");
    }

    [Test]
    public void Register_WithConfigObject_ShouldRegisterConfig()
    {
        var config = new MockConfig();

        configService.Register(config);

        Assert.IsTrue(ConfigService.Get<MockConfig>() != null, "Failed to register config");
    }

    [Test]
    public void SavesEnumerableToConfigObject()
    {
        configService.Register<ListConfig>("config.json");
        var config = (ListConfig)ConfigService.Get<ListConfig>();
        var expected = new List<int>{1,2,3,4,5};

        Assert.That(config.Answers.SequenceEqual(expected), "Failed to save enumerable to config object.");
    }

    [Test]
    [Ignore("")]
    public void Get_WithRegisteredConfig_ShouldReturnConfig()
    {
        configService.Register<MockConfig>("config.json");

        var config = ConfigService.Get<MockConfig>();

        Assert.IsNotNull(config, "Failed to get registered config");
    }

    [Test]
    [Ignore("")]
    public void Get_ConfigsFromConfigObject()
    {
        ConfigService.Unregister<MockConfig>();
        configService.Register<MockConfig>("config.json");

        MockConfig? config = (MockConfig)ConfigService.Get<MockConfig>();

        Assert.That(config.SERVER_IP == "127.0.0.1", "Config has wrong value for SERVER_IP.");
        Assert.That(config.SERVER_PORT == 12000, "Config has wrong value for SERVER_PORT.");
    }

    [Test]
    [Ignore("")]
    public void Get_WithUnregisteredConfig_ShouldReturnNull()
    {
        ConfigService.Unregister("MockConfig");
        var config = ConfigService.Get<MockConfig>();

        Assert.IsNull(config, "Failed to return null for unregistered config");
    }

    public class MockConfig : IConfig
    {
        override public string Name { get; } = "MockConfig";
        public override string FilePath => "config.json";
        public string SERVER_IP { get; set; } = "";
        public int SERVER_PORT { get; set; } = 0;
        public override string Section => "mockserver";
    }

    public class NestedConfig : IConfig
    {
        override public string Name { get; } = "NestedConfig";
        public override string FilePath => "config.json";
        public int AnswerIs { get; set; }
        public override string Section => "section/subsection/subsubsection";
    }
    public class ListConfig : IConfig
    {
        override public string Name { get; } = "ListConfig";
        public override string FilePath => "config.json";
        public IEnumerable<int> Answers { get; set; }
        public override string Section => "section/listconfig";
    }
}


//////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////

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


//////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////

[TestFixture]
public class Tests_Response
{
    public class TestModel : IModel
    {
        public int NotSerializedField { get; set; } = 100;
        public string SerializedField { get; set; } = "Serialized";
        public string? NullField { get; set; } = null;
        public object ToSerializableObj()
        {
            return new
            {
                SerializedField,
                NullField
            };
        }
        public TestModel() { }
    }

    public class TestModelRec : IModel
    {
        public int NotSerializedField { get; set; } = 100;
        public string SerializedField { get; set; } = "Serialized";
        public string? NullField { get; set; } = null;
        public IEnumerable<TestModel> SomeListItems { get; set; } = new List<TestModel> { new TestModel(), new TestModel() };
        public object ToSerializableObj()
        {
            return new
            {
                SerializedField,
                NullField,
                SomeListItems = SomeListItems.ToList().Select(c => c.ToSerializableObj()).ToList()
            };
        }

        public TestModelRec() { }
    }


    [Test]
    public void PayloadAsJson_ReturnsEmptyString_WhenPayloadIsNull()
    {
        var response = new Response<string>(200, "");

        string json = response.PayloadAsJson();

        Assert.AreEqual(string.Empty, json);
    }

    [Test]
    public void PayloadAsJson_ReturnsSerializedPayload_WhenPayloadIsIModel()
    {
        var testModel = new TestModel();
        var response = new Response<TestModel>(200, testModel, "");

        string json = response.PayloadAsJson();

        Assert.AreEqual("{\"SerializedField\":\"Serialized\",\"NullField\":null}", json);
    }

    [Test]
    public void Response_ReturnsSerializedPayload_WhenPayloadIsIModel_Recursivley()
    {
        var testModel = new TestModelRec();
        var response = new Response<TestModelRec>(200, testModel, "");

        string json = response.PayloadAsJson();

        Assert.AreEqual("{\"SerializedField\":\"Serialized\",\"NullField\":null,\"SomeListItems\":[{\"SerializedField\":\"Serialized\",\"NullField\":null},{\"SerializedField\":\"Serialized\",\"NullField\":null}]}", json);
    }


    [Test]
    public void StatusCode_ReturnsCorrectStatusCode()
    {
        var response = new Response<string>(200, "");

        Assert.AreEqual(200, response.StatusCode);
    }


    [Ignore("")]
    [Test]
    public void PayloadAsJson_ReturnsSerializedPayload_WhenPayloadIsNotIModel()
    {

    }
}

//////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////


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


}

//////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////

public class TestController : IController
{
    public TestController(IRequest request) : base(request) { }
    public void TestMethod()
    {

    }
}

//////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////

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

//////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////

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

//////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////

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

        mockEndpoint.Setup(m => m.UrlParams.NamedParams).Returns(new Dictionary<string, string> { { "userid", "897cb65a-4381-4c14-afda-65ff2cd291a4" } });

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

//////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////

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


[TestFixture]
public class Test_FileHandler
{
    [SetUp]
    public void Setup()
    {
        Directory.SetCurrentDirectory("/home/mustermax/vscode_projects/MTCG/MTCG.Tests/");
    }
    [Test]
    public void FileHandler_FindsKeyValuePair()
    {
        Directory.SetCurrentDirectory("/home/mustermax/vscode_projects/MTCG/MTCG.Tests/");

        string res = FileHandler.SearchKeyValueInFile("config.txt", "PRICE_VIP_PACKAGE");

        Assert.IsTrue(res == "15", $"Value is {res}");
    }

    [Test]
    public void FileHandler_GetsJsonObjectFromFile()
    {
        var res = FileHandler.ReadJsonFromFile("config.json");

        Assert.IsTrue(res!["mockserver"]["SERVER_IP"] == "127.0.0.1"
        && res!["mockserver"]["SERVER_PORT"] == 12000);
    }
}
//////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////





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
