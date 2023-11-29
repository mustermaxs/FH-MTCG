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
            mapper.RegisterEndpoint("/user/{userid:int}", HTTPMethod.GET, typeof(UserController), "Index");
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