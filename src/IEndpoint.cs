using System;

namespace MTCG;

public abstract class AbstractEndpoint
{
    public AbstractEndpoint(HTTPMethod method, string routePattern, Type controllerType, string controllerMethod)
    {
        if (!controllerType.IsAssignableFrom(typeof(IController)))
        {
            throw new ArgumentException($"Invalid controller type passed");
        }
        this.routePattern = routePattern;
        this.method = method;
        this.controllerMethod = controllerMethod;
        this.controllerType = controllerType;
    }
    protected string routePattern;
    protected string controllerMethod;
    protected Type controllerType;
    protected string ControllerMethod => controllerMethod;
    protected Type COntrollerType => controllerType;
    protected HTTPMethod method;
    public HTTPMethod Method => method;
    public string RoutePattern => routePattern;

}