using System;
using System.Security;
using System.Reflection;

namespace MTCG;

public class Router : IRouter
{
    private IEndpointMapper routeRegistry;
    private IUrlParser urlParser;
    private IAttributeHandler attributeHandler;
    private IRouteObtainer routeObtainer;
    // private IAuthenticator authenticator;

    public Router()
    {
        urlParser = new UrlParser();
        routeRegistry = RouteRegistry.GetInstance(urlParser);
        attributeHandler = new AttributeHandler();
        routeObtainer = new ReflectionRouteObtainer(attributeHandler);
    }

    public void RegisterRoutes()
    {
        var routes = routeObtainer.ObtainRoutes();

        foreach (var route in routes)
        {
            (string RouteTemplate, HTTPMethod HttpMethod, Type ControllerType, MethodInfo ControllerMethod) = route;
            routeRegistry.RegisterEndpoint(RouteTemplate, HttpMethod, ControllerType, ControllerMethod);
        }
    }

    /// 12.02.2023 21:23
    /// IMPROVE Refactoren
    public void HandleRequest(object sender, HttpSvrEventArgs svrEventArgs)
    {
        try
        {
            RoutingContext routeContext = new RoutingContext(svrEventArgs.Method, svrEventArgs.Path);
            routeContext.Payload = svrEventArgs.Payload;
            routeContext.Headers = svrEventArgs.Headers;
            routeRegistry.MapRequestToEndpoint(ref routeContext);
            Type controllerType = routeContext.Endpoint!.ControllerType;
            IController controller = (IController)Activator.CreateInstance(controllerType, routeContext);
            MethodInfo controllerAction = routeContext.Endpoint.ControllerMethod;

            IResponse response = controllerAction.MapArgumentsAndInvoke<IResponse, string>(controller, routeContext.Endpoint.UrlParams);
            Console.WriteLine(response.PayloadAsJson());
            svrEventArgs.Reply(200, response.PayloadAsJson());


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
            Console.WriteLine(ex);
        }
    }
}