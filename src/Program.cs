using System.Reflection;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MTCG
{
    public class Program
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // entry point                                                                                                      //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


        public static ServiceProvider services = new ServiceProvider();
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
            var serverConfig = new ServerConfig();
            var battleConfig = new BattleConfig();
            var responseConfig = new ResponseTextTranslator();
            var cardConfig = new CardConfig();

            serverConfig = serverConfig.Load<ServerConfig>();
            battleConfig = battleConfig.Load<BattleConfig>();
            responseConfig = responseConfig.Load<ResponseTextTranslator>();
            cardConfig = cardConfig.Load<CardConfig>();

            services
                .Register(serverConfig)
                .Register(battleConfig)
                .Register(responseConfig)
                .Register(cardConfig);
        }
    }
}