// using NUnit.Framework;
// using System;
// using System.Reflection;
// using MTCG;

// namespace UnitTests.MTCG;

// [TestFixture]
// public class AttributeRouting
// {
//     EndpointMapper? routeRegistry;
//     IAttributeHandler? attributeHandler;
//     IRouteObtainer? routeObtainer;
//     Exception? err;

//     [SetUp]
//     public void SetUp()
//     {
//         try
//         {
//             routeRegistry = new EndpointMapper(new UrlParser());
//             var currentAssembly = typeof(UserController).Assembly;
//             attributeHandler = new AttributeHandler(currentAssembly);
//             routeObtainer = new ReflectionRouteObtainer(attributeHandler);
//             var routes = routeObtainer.ObtainRoutes();

//             foreach (var route in routes)
//             {
//                 Console.WriteLine($"Route {route.RouteTemplate}");
//                 (HTTPMethod method, string routeTemplate, Type controllerType, string methodName) = route;
//                 routeRegistry.RegisterEndpoint(routeTemplate, method, controllerType, methodName);
//             }
//         }
//         catch (Exception ex)
//         {
//             Logger.Err(ex, true);
//         }
//     }
//     [Test]
//     public void RoutesGetRegisteredViaReflection()
//     {
//         Assert.IsTrue(routeRegistry?.IsRouteRegistered("user/{userid:int}", HTTPMethod.GET), $"");
//         Assert.IsTrue(this.routeRegistry?.IsRouteRegistered("user/{username:alpha}", HTTPMethod.GET), $"");
//     }
// }