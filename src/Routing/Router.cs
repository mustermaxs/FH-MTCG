using System;
using System.Security;
using System.Reflection;
using System.Threading.Tasks;
using Npgsql;


namespace MTCG;

//////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////

/// <summary>
/// Performs all the request/response handling.
/// Delegates the work to designated entities.
/// </summary>
public class Router : IRouter
{
    private IEndpointMapper routeRegistry;
    private IRouteObtainer routeObtainer;
    private ResponseTextTranslator languageConfig;


    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////


    /// <summary>
    /// Constructor for the router.
    /// Instantiates necessary dependencies like URL-Parser, RouteRegistry,
    /// AttributeHandler (for obtaining the endpoints to be registered via reflection/custom attributes).
    /// </summary>
    /// <param name="routeRegistry">
    /// An object implementing the IEndpointMapper interface.
    /// Responsible for mapping requests to registered endpoints.
    /// </param>
    /// <param name="routeObtainer">
    /// Gets 
    /// </param>
    public Router(IEndpointMapper routeRegistry, IRouteObtainer routeObtainer)
    {
        this.routeRegistry = routeRegistry;
        this.routeObtainer = routeObtainer;
        this.languageConfig = Program.services.Get<ResponseTextTranslator>();
    }


    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////


    /// <summary>
    /// Registeres the routes defined in the controller instances by
    /// looking for the RouteAttribute that marks a controller method as
    /// the responsible method for handling a specific client request.
    /// </summary>
    public void RegisterRoutes()
    {
        try
        {
            List<Endpoint> routes = routeObtainer.ObtainRoutes();

            foreach (var route in routes)
            {
                (
                    var RouteTemplate,
                    var HttpMethod,
                    var ControllerType,
                    var ControllerMethod,
                    var AccessLevel
                ) = route;

                var endpointBuilder = new EndpointBuilder();
                endpointBuilder
                .WithRouteTemplate(RouteTemplate)
                .WithHttpMethod(HttpMethod)
                .WithControllerType(ControllerType)
                .WithControllerMethod(ControllerMethod)
                .WithAccessLevel(AccessLevel);

                Endpoint endpoint = endpointBuilder.Build();
                routeRegistry.RegisterEndpoint(endpoint);
                Console.WriteLine($"[Register Route] {HttpMethod.ToString().PadRight(5)} {RouteTemplate}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Router failed to register routes.\n{ex}");
            throw new Exception($"{ex}");
        }

    }

    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////


    public bool ClientHasPermissionToRequest(IRequest request)
    {
        // Logger.ToConsole($"[Request]\t{request.HttpMethod} {request.RawUrl}", true);
        Role requestAccessLevel = request.Endpoint!.AccessLevel;
        Session session = null;

        if (requestAccessLevel == Role.ALL)
            return true;

        if (!request.TryGetHeader("Authorization", out string authToken)
            || !SessionManager.TryGetSessionWithToken(authToken, out session))
        {
            if (session == null && requestAccessLevel == Role.ANONYMOUS)
                return true;

            return false;
        }

        Role userAccessLevel = session.User!.UserAccessLevel;

        return userAccessLevel == (requestAccessLevel & userAccessLevel);
    }


    // FIXME ergibt irgendwie keinen Sinn, dass hier alles explizit gemacht werden soll
    protected void InitUserSettings(IRequest request)
    {
        var user = SessionManager.GetUserBySessionId(request.SessionId);
        languageConfig.SetLanguage(user!.Language);

    }

    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Responsible for getting the necessary ressources to process the request.
    /// Includes mapping the request to the responsible endpoint, instantiating the
    /// designated controller and executing the required controller method.
    /// Directly replies to the client.
    /// </summary>
    /// <param name="svrEventArgs">
    /// Object containing the received client request.
    /// </param>
    public async Task<IResponse> HandleRequest(IRequest request)
    {
        try
        {
            InitUserSettings(request);
            Logger.ToConsole($"[Map from]  {request.HttpMethod} {request.RawUrl}", true);
            routeRegistry.MapRequestToEndpoint(ref request);
            Logger.ToConsole($"[Map to]  {request.HttpMethod} {request.Endpoint!.RouteTemplate}\n", true);

            if (!ClientHasPermissionToRequest(request))
                throw new AuthenticationFailedException($"[DENIED]\tClient doesn't have access to ressource.\n{request.Payload}");

            var controllerType = request.Endpoint!.ControllerType;
            var controller = (IController?)Activator.CreateInstance(controllerType, request);

            if (controller == null)
                throw new Exception("Failed to instantiate controller.");

            MethodInfo controllerAction = request.Endpoint.ControllerMethod;
            IResponse? response = null;

            if (controllerAction.ReturnType == typeof(IResponse))
                response = controllerAction.MapArgumentsAndInvoke<IResponse, string>(controller, request.Endpoint.UrlParams.NamedParams);

            else
                response = await controllerAction.MapArgumentsAndInvokeAsync<IResponse, string>(controller, request.Endpoint.UrlParams.NamedParams);

            Logger.ToConsole($"Status: {response.StatusCode}\nResponse: {response.Description}", true);


            return response;
        }

        catch (RouteDoesntExistException ex)
        {
            Logger.ToConsole($"[ERROR]\n{ex}\nRoute doesn't exist: {ex.Url}", true);
            return new Response<string>(404, $"[ {ex.Url} ] {languageConfig["ROUTE_UNKOWN"]}");
        }

        catch (AuthenticationFailedException ex)
        {
            Logger.ToConsole($"[ERROR]\n{ex}\nAuthentication failed.", true);

            return new Response<string>(404, languageConfig["AUTH_ERR"]);
        }
        catch (Exception ex)
        {

            Logger.ToConsole($"[ERROR]\n{ex}\nSomething went wrong.", true);

            return new Response<string>(500, $"{languageConfig["INT_SVR_ERR"]}.\n{ex.Message}");
        }
        finally
        {
            // if anonymous session, end it
            var session = SessionManager.GetSessionById(request.SessionId);
            
            if (session.IsAnonymous)
                SessionManager.EndSessionWithSessionId(request.SessionId);
        }
    }
}