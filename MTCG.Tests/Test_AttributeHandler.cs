using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Reflection;
using MTCG;
using System.Linq;

namespace UnitTests.MTCG;



[Controller]
public class TestController_1 : BaseController
{
    public TestController_1(IRequest request) : base(request) { }
    [Route("/route/1", HTTPMethod.GET, Role.ALL)]
    public void Method_1(bool success) { }
    [Route("/route/2", HTTPMethod.PUT, Role.ALL)]
    public void Method_2() { }
    [Route("/route/3", HTTPMethod.POST, Role.ALL)]
    public void Method_3() { }
}


[TestFixture]
public class Test_AttributeHandler
{
    AttributeHandler attributeHandler;


    public Test_AttributeHandler()
    {
        this.attributeHandler = new(Assembly.GetExecutingAssembly());
    }


    [Test]

    public void AttributeHandler_FindsControllers()
    {
        
        var endpointList = new List<EndpointConfig>();

        var controllerTypes = attributeHandler.GetAttributesOfType<ControllerAttribute>();

        Assert.That(controllerTypes != null);
        Assert.That(controllerTypes!.Any(c => c.Name == "TestController_1"));
    }

    [Test]
    public void GetClassMethodsWithAttribute_GetsTestControllerMethods()
    {
        var methods = attributeHandler.GetClassMethodsWithAttribute<RouteAttribute>(typeof(TestController_1));
        string[] expectedMethods = { "Method_1", "Method_2", "Method_3" };

        Assert.That(methods != null);
        Assert.That(methods!.Count == 3);
        Assert.That(methods!.All(m => expectedMethods.Contains(m.Name)));
    }
}