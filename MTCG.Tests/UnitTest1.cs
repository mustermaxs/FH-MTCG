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




    [Ignore("")]
    [TestCase]
    public void HandleRequest_ReturnsResponseObject()
    {
        // var router = new Router(this.mockRouteRegistry, this.mockRouteObtainer);

        // IResponse response = router.HandleRequest(this.mockRequest);

        // Assert.IsTrue(response.PayloadAsJson() == this.mockRequest.Payload, $"{response.PayloadAsJson()} != {mockRequest.Payload}");
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


//////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////

[TestFixture]
public class Test_BattleConfig
{
    [SetUp]
    public void Setup()
    {
        Directory.SetCurrentDirectory("/home/mustermax/vscode_projects/MTCG/MTCG.Tests/");
    }

    [TestCase("Goblin", "english", "Goblins are too afraid of Dragons to attack.")]
    [TestCase("Dragon", "german", "Die Feuerelfen kennen Drachen seit ihrer Kindheit und können ihren Angriffen ausweichen.")]
    public void GetTranslationsFromConfig(string monster, string language, string description)
    {
        var config = JsonConfigLoader.Load<BattleConfig>("config.json", "battle");
        string? txt = config!.CardDescription(monster, language);

        if (txt == string.Empty) Assert.Fail();

        Assert.That(txt == description, "Failed to get description.");
    }

    [TestCase("Goblin", "Dragon", false)]
    public void GetPropertiesForMonster(string attacker, string defender, bool wins)
    {
        var config = JsonConfigLoader.Load<BattleConfig>("config.json", "battle");
        var properties = config!.GetMonsterProperties(attacker);

        if (properties == null) Assert.Fail();

        (string player, string opponent, bool win) = properties.Value;

        Assert.That(opponent == defender, "Failed to get properties.");
        Assert.That(win == wins, "Failed to get properties.");
        Assert.That(player == attacker, "Failed to get properties.");
    }
}