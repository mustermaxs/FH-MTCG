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


            /// <summary>
            /// Setup necessary entities for mapping of endpoints,
            /// instantiating and starting server and router.
            /// This application currently uses reflection and custom attributes
            /// to register endpoints and the responsible controllers
            /// to handle requests.
            /// </summary>
            LoadConfigs();
            IUrlParser urlParser = new UrlParser();
            IEndpointMapper routeRegistry = RouteRegistry.GetInstance(urlParser);
            IAttributeHandler attributeHandler = new AttributeHandler();
            IRouteObtainer routeObtainer = new ReflectionRouteObtainer(attributeHandler);

            Router router = new(routeRegistry, routeObtainer);
            HttpServer svr = new(router);
            svr.Run();
        }

        protected static void LoadConfigs()
        {
            // Directory.SetCurrentDirectory("/home/mustermax/vscode_projects/MTCG/MTCG.Tests/");
            var configService = ConfigService.GetInstance();

            configService
                .Register<ServerConfig>(null)
                .Register<UserConfig>(null)
                .Register<ResponseConfig>(null)
                .Register<CardConfig>(null);
        }
    }
}