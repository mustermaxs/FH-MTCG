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
            // HttpServer svr = new();
            // svr.Incoming += _ProcessMesage;

            // svr.Run();
            IUrlParser parser = new UrlParser();
            RouteRegistry registry = new RouteRegistry(parser);
            registry.RegisterGet("api/{controller:alpha}/user/{id:int}");
            registry.RegisterGet("api/score/user/{id:int}");
            GroupCollection m1 = registry.Map("/api/home/user/2", HTTPMethod.GET);
            GroupCollection m2 = registry.Map("/api/score/user/21", HTTPMethod.GET);
            
            if (m1 != null)
            {
                Console.WriteLine(m1["controller"].Value);
            }
            // Assembly assembly = Assembly.GetExecutingAssembly();
            // var controllerTypes = assembly.GetTypes().Where(t => t.GetCustomAttribute<ControllerAttribute>() != null);

            // foreach (var controllerType in controllerTypes)
            // {
            //     IEnumerable<MethodInfo> methods = controllerType.GetMethods()
            //         .Where(method => method.GetCustomAttribute<RouteAttribute>() != null);

            //     foreach (var controllerMethod in methods)
            //     {
            //         Console.WriteLine(controllerMethod.Name);
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