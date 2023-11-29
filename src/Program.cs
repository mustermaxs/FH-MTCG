﻿using System.Reflection;
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
            // HttpServer svr = new();
            // svr.Incoming += _ProcessMesage;

            // svr.Run();
            string requestedRoute = "/user/mustermax";
            HTTPMethod reqMethod = HTTPMethod.GET;
            IUrlParser parser = new UrlParser();
            RouteResolver routeResolver = new RouteResolver(parser);

            Assembly assembly = Assembly.GetExecutingAssembly();
            var controllerTypes = assembly.GetTypes().Where(t => t.GetCustomAttribute<ControllerAttribute>() != null);

            foreach (Type controllerType in controllerTypes)
            {
                IEnumerable<MethodInfo> routeActions = controllerType.GetMethods()
                    .Where(method => method.GetCustomAttribute<RouteAttribute>() != null);

                foreach (MethodInfo method in controllerType.GetMethods())
                {
                    RouteAttribute routeAttribute = (RouteAttribute)Attribute.GetCustomAttribute(method, typeof(RouteAttribute));

                    if (routeAttribute != null)
                    {
                        string route = routeAttribute.Route;
                        HTTPMethod httpMethod = routeAttribute.Method;

                        routeResolver.RegisterRoute(route, httpMethod);
                        ResolvedUrl req = routeResolver.MapRequest(requestedRoute, reqMethod);
                        
                        if (req.IsRouteRegistered)
                        {
                            Console.WriteLine($"is route registered: {req.IsRouteRegistered}");

                        }
                    }
                }
            }

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