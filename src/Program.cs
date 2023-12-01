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

            HttpServer svr = new();
            svr.Incoming += HandleClientRequest;

            svr.Run();

        }

        private static void HandleClientRequest(object sender, HttpSvrEventArgs e)
        {
            try
            {
            var routeRegistry = EndpointMapper.GetInstance();
            TokenizedUrl urlObj = routeRegistry.TryMapRouteToEndpoint(e.Path, e.Method);
            MethodInfo action = urlObj.Endpoint.ControllerMethod;
            Type controllerType = urlObj.Endpoint.ControllerType;
            var controller = (IController)Activator.CreateInstance(controllerType);
            var attributeHandler = new AttributeHandler(Assembly.GetExecutingAssembly());
            var response = action.MapArgumentsAndInvoke<string, string>(controller, urlObj.UrlParams);
            e.Reply(200, response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex}");
                e.Reply(400, "Error");
            }

        }


        /// <summary>Event handler for incoming server requests.</summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event arguments.</param>
        // private static void _ProcessMesage(object sender, HttpSvrEventArgs e)
        // {
        //     Console.WriteLine(e.PlainMessage);

        //     e.Reply(200, "Yo! Understood.");
        // }
    }
}