using NUnit.Framework;
using MTCG;
using NUnit.Framework.Internal;
using Moq;
using System.Collections.Generic;
using System.Linq;

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


[Controller]
public class ReflectionRouteObtainerTest : IController
{
    public ReflectionRouteObtainerTest(IRequest request) : base(request) { }

    [Route("/test/route", HTTPMethod.GET, Role.USER)]
    public void TestMethod()
    {
    }
}
