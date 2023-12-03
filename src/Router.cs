using System;
using System.Reflection;

namespace MTCG;

public class Router : IRouter
{
    private IEndpointMapper routeRegistry;
    private IUrlParser urlParser;
    private IAttributeHandler attributeHandler;
    private IRouteObtainer routeObtainer;

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
        string requestedUrl = svrEventArgs.Path;
        HTTPMethod httpMethod = svrEventArgs.Method;
        RoutingContext routeContext = new RoutingContext(httpMethod, requestedUrl);
        routeRegistry.MapRequestToEndpoint(ref routeContext);
        routeContext.Headers = svrEventArgs.Headers;
        Type controllerType = routeContext.Endpoint!.ControllerType;
        IController controller = (IController)Activator.CreateInstance(controllerType, routeContext);
        MethodInfo controllerAction = routeContext.Endpoint.ControllerMethod;
        var response = controllerAction.MapArgumentsAndInvoke<string, string>(controller, routeContext.Endpoint.UrlParams);
        Console.WriteLine(response);
        svrEventArgs.Reply(200, response);
    }
}