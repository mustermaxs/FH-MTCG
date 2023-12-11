using System;
using System.Security;
using System.Reflection;
using System.Threading.Tasks;

namespace MTCG;




/// <summary>
/// Performs all the request/response handling.
/// Delegates the work to designated entities.
/// </summary>

public class Router : IRouter
{
    private IEndpointMapper routeRegistry;
    private IRouteObtainer routeObtainer;
    // private IAuthenticator authenticator;



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


    /// 06.12.2023 20:40
    /// IMPROVE routeObtainer sollte nicht direkt im router verwendet werden
    /// router sollte in RegisterRoutes
    public Router(IEndpointMapper routeRegistry, IRouteObtainer routeObtainer)
    {
        this.routeRegistry = routeRegistry;
        this.routeObtainer = routeObtainer;
    }




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
            Console.WriteLine(endpoint.RouteTemplate);
            routeRegistry.RegisterEndpoint(endpoint);
        }
    }




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
            Console.WriteLine($"Authentication failed.");

            return new ResponseWithoutPayload(404, $"Something went wrong.\n{ex.Message}");
        }

        catch (Exception ex)
        {
            Console.WriteLine($"ERROR\n{ex}");
            
            return new ResponseWithoutPayload(500, $"Something went wrong.\n{ex.Message}");
        }
    }
}