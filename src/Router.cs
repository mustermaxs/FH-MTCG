using System;
using System.Security;
using System.Reflection;
using System.Threading.Tasks;

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
    // private IAuthenticator authenticator;


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

    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////

    protected bool CompareAccessLevels(Role clientAccessLevel, Role requiredAccessLevel)
    {
        return clientAccessLevel >= requiredAccessLevel;
    }

    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////


    public bool ClientHasPermissionToRequest(IRequest request)
    {
        Role requestAccessLevel = request.Endpoint!.AccessLevel;

        Session session = null;


        if (requestAccessLevel == Role.ALL)
        {
            return true;
        }


        if (!request.TryGetHeader("Authorization", out string authToken)
            || !SessionManager.TryGetSessionWithAuthToken(authToken, out session))
        {
            if (session == null && requestAccessLevel == Role.ANONYMOUS) return true;

            return false;
        }

        Role userAccessLevel = session.User!.GetUserAccessLevel();

        return userAccessLevel == (requestAccessLevel & userAccessLevel);
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
    public IResponse HandleRequest(ref IRequest request)
    {
        try
        {
            routeRegistry.MapRequestToEndpoint(ref request);

            if (!ClientHasPermissionToRequest(request)) throw new AuthenticationFailedException($"Client doesn't have access to ressource.");

            var controllerType = request.Endpoint!.ControllerType;
            var controller = (IController)Activator.CreateInstance(controllerType, request);

            if (controller == null)
            {
                throw new Exception("Failed to instantiate controller.");
            }


            MethodInfo controllerAction = request.Endpoint.ControllerMethod;
            IResponse response = controllerAction.MapArgumentsAndInvoke<IResponse, string>(controller, request.Endpoint.UrlParams);

            Console.WriteLine($"Response: {response.Description}\nStatus: {response.StatusCode}");

            return response;
        }

        catch (RouteDoesntExistException ex)
        {
            Console.WriteLine($"{ex}\nRequested endpoint: {ex.Url}");

            return new ResponseWithoutPayload(404, $"The requested endpoint {ex.Url} doesn't seem to exist.");
        }

        catch (AuthenticationFailedException ex)
        {
            Console.WriteLine($"Access token is missing or invalid");

            return new ResponseWithoutPayload(404, $"Access token is missing or invalid.\n{ex.Message}");
        }

        catch (Exception ex)
        {
            Console.WriteLine($"ERROR\n{ex}");

            return new ResponseWithoutPayload(500, $"Something went wrong.\n{ex.Message}");
        }
    }
}