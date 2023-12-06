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
        var routes = routeObtainer.ObtainRoutes();

        foreach (var route in routes)
        {
            (string RouteTemplate,
            HTTPMethod HttpMethod,
            Type ControllerType,
            MethodInfo ControllerMethod) = route;

            routeRegistry.RegisterEndpoint(RouteTemplate, HttpMethod, ControllerType, ControllerMethod);
        }
    }



    /// <summary>
    /// Creates the context for a specific client request.
    /// </summary>
    /// <param name="svrEventArgs">
    /// Object that contains all the information received via the HttpSvr class.
    /// </param>
    /// <returns>
    /// RoutingContext instance specific to a single client request.
    /// </returns>

    /// 06.12.2023 20:42
    /// IMPROVE RoutingContext irgedwo anders konstruieren und
    /// statt svrEventArgs in HandleRequest übergeben
    // protected RoutingContext CreateRoutingContext(HttpSvrEventArgs svrEventArgs)
    // {
    //     var context = new RoutingContext(svrEventArgs.Method, svrEventArgs.Path);
    //     context.Payload = svrEventArgs.Payload;
    //     context.Headers = svrEventArgs.Headers;

    //     return context;
    // }




    /// <summary>
    /// Responsible for getting the necessary ressources to process the request.
    /// Includes mapping the request to the responsible endpoint, instantiating the
    /// designated controller and executing the required controller method.
    /// Directly replies to the client.
    /// </summary>
    /// <param name="svrEventArgs">
    /// Object containing the received client request.
    /// </param>

    /// 06.12.2023 20:42
    /// IMPROVE RoutingContext irgedwo anders konstruieren und
    /// statt svrEventArgs in HandleRequest übergeben
    public IResponse HandleRequest(ref IRoutingContext context)
    {
        try
        {
            routeRegistry.MapRequestToEndpoint(ref context);

            var controllerType = context.Endpoint!.ControllerType;
            var controller = (IController)Activator.CreateInstance(controllerType, context);

            if (controller == null) throw new Exception("Failed to instantiate controller.");

            MethodInfo controllerAction = context.Endpoint.ControllerMethod;

            IResponse response = controllerAction.MapArgumentsAndInvoke<IResponse, string>(controller, context.Endpoint.UrlParams);

            // svrEventArgs.Reply(response.StatusCode, response.PayloadAsJson());

            return response;
        }
        catch (DbTransactionFailureException ex)
        {
            Console.WriteLine($"Transaction failed.\n{ex.Message}");

            return new ErrorResponse<string>($"Transaction failed.", 500);

        }
        catch (RouteDoesntExistException ex)
        {
            Console.WriteLine($"{ex}\nRequested endpoint: {ex.Url}");

            return new ErrorResponse<string>($"The requested endpoint {ex.Url} doesn't seem to exist.", 404);
            // svrEventArgs.Reply(404, $"The requested endpoint {ex.Url} doesn't seem to exist.");
        }
        catch (AuthenticationFailedException ex)
        {
            Console.WriteLine($"Authentication failed.");

            return new ErrorResponse<string>($"Something went wrong", 404);

            // svrEventArgs.Reply(401, $"Something went wrong");
        }
        catch (Exception ex)
        {

            return new ErrorResponse<string>($"Something went wrong. {ex}", 500);

            // svrEventArgs.Reply(500, $"Something went wrong. {ex}");
        }
    }
}