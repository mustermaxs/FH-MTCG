using System;
using System.Data.SqlTypes;
using System.Reflection;

namespace MTCG;

public abstract class IEndpoint
{
    public IEndpoint(HTTPMethod httpMethod, string? routePattern, string routeTemplate, Type controllerType, MethodInfo controllerMethod)
    {
        if (!typeof(IController).IsAssignableFrom(controllerType))
        {
            throw new ArgumentException($"Invalid controller type passed.\nType passed: {controllerType}");
        }
        this.endpointPattern = routePattern ?? null;
        this.httpMethod = httpMethod;
        this.controllerMethod = controllerMethod;
        this.controllerType = controllerType;
        this.routeTemplate = routeTemplate;
    }

    virtual public void Deconstruct(out string routeTemplate, out HTTPMethod httpMethod, out Type controllerType, out MethodInfo controllerMethod)
    {
        routeTemplate = RouteTemplate;
        httpMethod = Httpmethod;
        controllerType = ControllerType;
        controllerMethod = ControllerMethod;
    }

    public override bool Equals(Object obj)
    {
        if (obj == null) return false;
        IEndpoint endpoint = (IEndpoint)obj;

        return
            Httpmethod == (HTTPMethod)endpoint.Httpmethod &&
            RouteTemplate == (string)endpoint.RouteTemplate &&
            ControllerType == (Type)endpoint.ControllerType &&
            ControllerMethod == (MethodInfo)endpoint.ControllerMethod;
    }

    public static bool operator ==(IEndpoint e1, IEndpoint e2)
    {
        if (e1 == null || e2 == null) return false;

        return
            e1.Httpmethod == (HTTPMethod)e2.Httpmethod &&
            e1.RouteTemplate == (string)e2.RouteTemplate &&
            e1.ControllerType == (Type)e2.ControllerType &&
            e1.ControllerMethod == (MethodInfo)e2.ControllerMethod;
    }

    public static bool operator !=(IEndpoint e1, IEndpoint e2)
    {
        if (e1 == null && e2 == null) return false;
        else if (e1 == null || e2 == null) return true;

        return
            e1.Httpmethod != (HTTPMethod)e2.Httpmethod ||
            e1.RouteTemplate != (string)e2.RouteTemplate ||
            e1.ControllerType != (Type)e2.ControllerType ||
            e1.ControllerMethod != (MethodInfo)e2.ControllerMethod;
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }


    protected string? endpointPattern;
    protected string routeTemplate;
    protected MethodInfo controllerMethod;
    protected Type controllerType;
    protected HTTPMethod httpMethod;
    public string RouteTemplate => routeTemplate;
    public MethodInfo ControllerMethod => controllerMethod;
    public Type ControllerType => controllerType;
    public HTTPMethod Httpmethod => httpMethod;
    public string EndpointPattern => endpointPattern;
}