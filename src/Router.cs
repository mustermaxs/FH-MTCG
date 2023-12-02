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
            (string routeTemplate, HTTPMethod httpMethod, Type controllerType, MethodInfo controllerMethod) = route;
            routeRegistry.RegisterEndpoint(routeTemplate, httpMethod, controllerType, controllerMethod);
        }
    }

    public void HandleRequest(object sender, HttpSvrEventArgs svrEventArgs)
    {
        string requestedUrl = svrEventArgs.Path;
        HTTPMethod httpMethod = svrEventArgs.Method;
        RoutingContext? routeContext = routeRegistry.MapRequestToEndpoint(requestedUrl, httpMethod);
        Type controllerType = routeContext.Endpoint!.ControllerType;
        IController controller = (IController)Activator.CreateInstance(controllerType, routeContext);
        MethodInfo controllerAction = routeContext.Endpoint.ControllerMethod;
        var response = controllerAction.MapArgumentsAndInvoke<string, string>(controller, routeContext.UrlParams);
        Console.WriteLine(response);
        svrEventArgs.Reply(200, response);
    }
}