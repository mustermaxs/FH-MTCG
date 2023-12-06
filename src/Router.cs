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
    private IUrlParser urlParser;
    private IAttributeHandler attributeHandler;
    private IRouteObtainer routeObtainer;
    // private IAuthenticator authenticator;



    /// <summary>
    /// Constructor for the router.
    /// Instantiates necessary dependencies like URL-Parser, RouteRegistry,
    /// AttributeHandler (for obtaining the endpoints to be registered via reflection/custom attributes).
    /// </summary>
    
    public Router()
    {
        urlParser = new UrlParser();
        routeRegistry = RouteRegistry.GetInstance(urlParser);
        attributeHandler = new AttributeHandler();
        routeObtainer = new ReflectionRouteObtainer(attributeHandler);
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
    
    protected RoutingContext CreateRoutingContext(HttpSvrEventArgs svrEventArgs)
    {
        var context = new RoutingContext(svrEventArgs.Method, svrEventArgs.Path);
        context.Payload = svrEventArgs.Payload;
        context.Headers = svrEventArgs.Headers;

        return context;
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
    
    public void HandleRequest(HttpSvrEventArgs svrEventArgs)
    {
        try
        {
            var context = this.CreateRoutingContext(svrEventArgs);
            routeRegistry.MapRequestToEndpoint(ref context);

            var controllerType = context.Endpoint!.ControllerType;
            var controller = (IController)Activator.CreateInstance(controllerType, context);

            if (controller == null) throw new Exception("Failed to instantiate controller.");

            MethodInfo controllerAction = context.Endpoint.ControllerMethod;

            IResponse response = controllerAction.MapArgumentsAndInvoke<IResponse, string>(controller, context.Endpoint.UrlParams);

            svrEventArgs.Reply(response.StatusCode, response.PayloadAsJson());
        }
        catch (DbTransactionFailureException ex)
        {
            Console.WriteLine($"Transaction failed.\n{ex.Message}");
        }
        catch (RouteDoesntExistException ex)
        {
            Console.WriteLine($"{ex}\nRequested endpoint: {ex.Url}");
            svrEventArgs.Reply(404, $"The requested endpoint {ex.Url} doesn't seem to exist.");
        }
        catch (AuthenticationFailedException ex)
        {
            Console.WriteLine($"Authentication failed.");
            svrEventArgs.Reply(401, $"Something went wrong");
        }
        catch (Exception ex)
        {
            svrEventArgs.Reply(500, $"Something went wrong. {ex}");
        }
    }
}