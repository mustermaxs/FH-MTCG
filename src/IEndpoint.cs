using System;

namespace MTCG;

public abstract class AbstractEndpoint
{
    public AbstractEndpoint(HTTPMethod method, string routePattern, string routeTemplate, Type controllerType, string controllerMethod)
    {
        if (!typeof(IController).IsAssignableFrom(controllerType))
        {
            throw new ArgumentException($"Invalid controller type passed.\nType passed: {controllerType}");
        }
        this.endpointPattern = routePattern;
        this.method = method;
        this.controllerMethod = controllerMethod;
        this.controllerType = controllerType;
        this.routeTemplate = routeTemplate;
    }
    protected string endpointPattern;
    protected string routeTemplate;
    public string RouteTemplate => routeTemplate;
    protected string controllerMethod;
    protected Type controllerType;
    public string ControllerMethod => controllerMethod;
    public Type ControllerType => controllerType;
    protected HTTPMethod method;
    public HTTPMethod Method => method;
    public string EndpointPattern => endpointPattern;
}