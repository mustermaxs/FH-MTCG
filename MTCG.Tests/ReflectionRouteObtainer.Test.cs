using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Reflection;
using MTCG;
using System.Linq;

namespace UnitTests.MTCG;






[TestFixture]
public class Test_CustomReflexionExtension
{
    [Test]
    public void InvokePassesArgumentsToMethodAndGetCorrectReturnValue()
    {
        var obj = new TestClassForCustomInvoker();
        MethodInfo? methodInfo = typeof(TestClassForCustomInvoker).GetMethod("TestMethodForCustomInvokeExpectTrue");
        Dictionary<string, string> providedParams = new();
        providedParams.Add("gut", "true");
        providedParams.Add("goesser", "true");
        providedParams.Add("besser", "true");

        bool result = methodInfo!.MapArgumentsAndInvoke<bool, string>(obj, providedParams);

        Assert.IsTrue(result, $"Something went wrong");
    }
}

public class TestClassForCustomInvoker
{
    public bool TestMethodForCustomInvokeExpectTrue(bool gut, bool besser, bool goesser)
    {
        return (gut && besser && goesser);
    }
}
