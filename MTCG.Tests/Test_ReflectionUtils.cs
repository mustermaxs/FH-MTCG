using NUnit.Framework;
using System;
using MTCG;
using System.Reflection;
using NUnit.Framework.Internal;
using System.Collections.Generic;

namespace UnitTests.MTCG;

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
