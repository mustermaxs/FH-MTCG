using System.Reflection;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace MTCG
{
    internal class Program
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // entry point                                                                                                      //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Main entry point.</summary>
        /// <param name="args">Arguments.</param>
        static void Main(string[] args)
        {
            EndpointMapper mapper = new(new UrlParser());

            var currentAssembly = Assembly.GetExecutingAssembly();
            var controllerManager = new AttributeHandler(currentAssembly);
            var controllerTypes = controllerManager.GetAttributeOfType<ControllerAttribute>(typeof(IController));

            foreach (var controllerType in controllerTypes)
            {
                var methods = controllerManager.GetClassMethodsWithAttribute<RouteAttribute>(controllerType);

                foreach (var method in methods)
                {
                    var routeAttribute = controllerManager.GetMethodAttributeWithMethodInfo<RouteAttribute>(method);
                    mapper.RegisterEndpoint(routeAttribute.RouteTemplate, routeAttribute.Method, controllerType, method.Name);
                }
            }
            var req = mapper.TryMapRouteToEndpoint("/user/Maximilian", HTTPMethod.GET);
            IController controller = (IController)Activator.CreateInstance(req.Endpoint.ControllerType);
            string methodName = req.Endpoint.ControllerMethod;
            MethodInfo controllerMethod = req.Endpoint.ControllerType.GetMethod(methodName);
            // object {username} = req.
            Console.WriteLine(controllerMethod.Invoke(controller, new object[]{req.GetParam<string>("username")}));
            // HttpServer svr = new();
            // svr.Incoming += _ProcessMesage;


            // svr.Run();
            // string requestedRoute = "/user/mustermax";
            // HTTPMethod reqMethod = HTTPMethod.GET;
            // IUrlParser parser = new UrlParser();
            // EndpointMapper routeResolver = new EndpointMapper(parser);

            // Assembly assembly = Assembly.GetExecutingAssembly();
            // var controllerTypes = assembly.GetTypes().Where(t => t.GetCustomAttribute<ControllerAttribute>() != null);

            // foreach (Type controllerType in controllerTypes)
            // {
            //     IEnumerable<MethodInfo> routeActions = controllerType.GetMethods()
            //         .Where(method => method.GetCustomAttribute<RouteAttribute>() != null);

            //     foreach (MethodInfo method in controllerType.GetMethods())
            //     {
            //         RouteAttribute routeAttribute = (RouteAttribute)Attribute.GetCustomAttribute(method, typeof(RouteAttribute));

            //         if (routeAttribute != null)
            //         {
            //             string route = routeAttribute.Route;
            //             HTTPMethod httpMethod = routeAttribute.Method;

            //             routeResolver.RegisterEndpoint(route, httpMethod);
            //             RequestObject req = routeResolver.TryMapRouteToEndpoint(requestedRoute, reqMethod);

            //             if (req.RouteFound)
            //             {
            //                 Console.WriteLine($"is route registered: {req.RouteFound}");

            //             }
            //         }
            //     }
            // }

        }


        /// <summary>Event handler for incoming server requests.</summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event arguments.</param>
        private static void _ProcessMesage(object sender, HttpSvrEventArgs e)
        {
            Console.WriteLine(e.PlainMessage);

            e.Reply(200, "Yo! Understood.");
        }
    }
}