// using NUnit.Framework;
// using System;
// using System.Collections.Generic;
// using System.Reflection;
// using MTCG;

// namespace UnitTests.MTCG
// {


//     [TestFixture]
//     public class Test_AttributeHandler
//     {
//         public static List<EndpointConfig>? expectedEndpointConfigs;
//         [SetUp]
//         public void SetUp()
//         {
//             expectedEndpointConfigs = new List<EndpointConfig>{
//                 new EndpointConfig
//             {
//                 Method = HTTPMethod.GET,
//                 RouteTemplate = "/route/1",
//                 ControllerType = typeof(TestController_1),
//                 ControllerMethod = "Method_1"
//             },
//                 new EndpointConfig
//             {
//                 Method = HTTPMethod.PUT,
//                 RouteTemplate = "/route/2",
//                 ControllerType = typeof(TestController_1),
//                 ControllerMethod = "Method_2"
//             },
//                 new EndpointConfig
//             {
//                 Method = HTTPMethod.POST,
//                 RouteTemplate = "/route/3",
//                 ControllerType = typeof(TestController_1),
//                 ControllerMethod = "Method_3"
//             },

//             };
//         }

//         private static IEnumerable<TestCaseData> ExpectedEndpointConfigs()
//         {
//             // Return TestCaseData instances with your test cases
//             yield return new TestCaseData(expectedEndpointConfigs?[0]);
//             yield return new TestCaseData(expectedEndpointConfigs?[1]);
//             yield return new TestCaseData(expectedEndpointConfigs?[2]);
//         }

//         // [TestCaseSource(nameof(ExpectedEndpointConfigs))]
//         [Test]

//         public void AttributeHandler_FindsControllerAndRegisteredRoutes()
//         {
//             AttributeHandler attributeHandler = new(Assembly.GetExecutingAssembly());
//             var endpointList = new List<EndpointConfig>();

//             var controllerTypes = attributeHandler.GetAttributeOfType<ControllerAttribute>(typeof(IController));

//             foreach (var controllerType in controllerTypes)
//             {
//                 var controllerMethodsInfos = attributeHandler.GetClassMethodsWithAttribute<RouteAttribute>(controllerType);

//                 foreach (var methodInfo in controllerMethodsInfos)
//                 {
//                     var routeAttribute = attributeHandler.GetMethodAttributeWithMethodInfo<RouteAttribute>(methodInfo);
//                     var endpointConfig = new EndpointConfig
//                     {
//                         Method = (HTTPMethod)routeAttribute?.Method,
//                         RouteTemplate = routeAttribute?.RouteTemplate,
//                         ControllerType = (Type)controllerType,
//                         ControllerMethod = methodInfo.Name
//                     };

//                     endpointList.Add(endpointConfig);
//                 }
//             }
//             // Assertions

//             // Check if there are controller types with ControllerAttribute
//             Assert.IsNotEmpty(controllerTypes);

//             // Check if there are registered routes
//             Assert.IsNotEmpty(endpointList);

//             // Example: Assert for a specific route registration
//             var expectedEndpointConfigs = new List<EndpointConfig>{
//                 new EndpointConfig
//             {
//                 Method = HTTPMethod.GET,
//                 RouteTemplate = "/route/1",
//                 ControllerType = typeof(TestController_1),
//                 ControllerMethod = "Method_1"
//             },
//                 new EndpointConfig
//             {
//                 Method = HTTPMethod.PUT,
//                 RouteTemplate = "/route/2",
//                 ControllerType = typeof(TestController_1),
//                 ControllerMethod = "Method_2"
//             },
//                 new EndpointConfig
//             {
//                 Method = HTTPMethod.POST,
//                 RouteTemplate = "/route/3",
//                 ControllerType = typeof(TestController_1),
//                 ControllerMethod = "Method_3"
//             },

//             };

//             foreach (EndpointConfig e in expectedEndpointConfigs)
//             {
//                 Assert.That(endpointList.Exists(endpoint => endpoint.Equals(e)), $"Endpoint not found!");
//             }
//         }
//     }

//     // You may add more test methods for specific scenarios or edge cases


//     [Controller]
//     public class TestController_1 : IController
//     {
//         [Route("/route/1", HTTPMethod.GET)]
//         public void Method_1(bool success) { }
//         [Route("/route/2", HTTPMethod.PUT)]
//         public void Method_2() { }
//         [Route("/route/3", HTTPMethod.POST)]
//         public void Method_3() { }
//     }


//     [TestFixture]
//     public class Test_CustomReflexionExtension
//     {
//         [Test]
//         public void InvokePassesArgumentsToMethodAndGetCorrectReturnValue()
//         {
//             var obj = new TestClassForCustomInvoker();
//             MethodInfo? methodInfo = typeof(TestClassForCustomInvoker).GetMethod("TestMethodForCustomInvokeExpectTrue");
//             Dictionary<string, string> providedParams = new();
//             providedParams.Add("gut", "true");
//             providedParams.Add("goesser", "true");
//             providedParams.Add("besser", "true");

//             bool result = methodInfo!.MapArgumentsAndInvoke<bool, string>(obj, providedParams);

//             Assert.IsTrue(result, $"Something went wrong");
//         }
//     }

//     public class TestClassForCustomInvoker
//     {
//         public bool TestMethodForCustomInvokeExpectTrue(bool gut, bool besser, bool goesser)
//         {
//             return (gut && besser && goesser);
//         }
//     }
// }
