using System.Reflection;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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
            IUrlParser urlParser = new UrlParser();
            IEndpointMapper routeRegistry = RouteRegistry.GetInstance(urlParser);
            IAttributeHandler attributeHandler = new AttributeHandler();
            IRouteObtainer routeObtainer = new ReflectionRouteObtainer(attributeHandler);
            
            var router = new Router();
            HttpServer svr = new(router);
            svr.Run();

            // ConstructorInfo info = typeof(User).GetConstructors()[0];
            // User user = info.MapArgumentsAndCreateInstance<User>();
            // Console.WriteLine("test");
        }
    }
}