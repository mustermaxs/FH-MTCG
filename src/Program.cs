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

        public static ServiceProvider services = new ServiceProvider();

        
        /// <summary>Main entry point.</summary>
        /// <param name="args">Arguments.</param>
        static void Main(string[] args)
        {
            /// Setup necessary entities for mapping of endpoints,
            /// instantiating and starting server and router.
            /// This application currently uses reflection and custom attributes
            /// to register endpoints and the responsible controllers
            /// to handle requests.
            
            LoadServices();
            IUrlParser urlParser = new UrlParser();
            IEndpointMapper routeRegistry = RouteRegistry.GetInstance(urlParser);
            IAttributeHandler attributeHandler = new AttributeHandler();
            IRouteObtainer routeObtainer = new ReflectionRouteObtainer(attributeHandler);

            Router router = new(routeRegistry, routeObtainer);
            HttpServer svr = new(router);
            svr.Run();
        }

        protected static void LoadServices()
        {
            var serverConfig = new ServerConfig();
            var battleConfig = new BattleConfig();
            var responseConfig = new ResponseTextTranslator();
            var cardConfig = new CardConfig();

            serverConfig = serverConfig.Load<ServerConfig>(Constants.CONFIG_FILE_PATH);
            battleConfig = battleConfig.Load<BattleConfig>(Constants.CONFIG_FILE_PATH);
            responseConfig = responseConfig.Load<ResponseTextTranslator>(Constants.CONFIG_FILE_PATH);
            cardConfig = cardConfig.Load<CardConfig>(Constants.CONFIG_FILE_PATH);

            

            services
                .Register(serverConfig)
                .Register(responseConfig)
                .Register(battleConfig)
                .RegisterDisposable<CardRepository>()
                .RegisterDisposable<BattleRepository>()
                .RegisterDisposable<BattleLogRepository>()
                .RegisterDisposable<UserRepository>()
                .RegisterDisposable<PackageRepository>()
                .RegisterDisposable<CardTradeRepository>()
                .Register(cardConfig);
        }
    }
}