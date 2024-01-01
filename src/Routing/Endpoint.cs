using System;
using System.Reflection;

namespace MTCG;

/// 08.12.2023 13:56
/// DOCUMENT

public class Endpoint : IEndpoint
{



    public Endpoint(HTTPMethod HttpMethod, string? routePattern, string RouteTemplate, Type controllerType, MethodInfo controllerMethod, Role accessLevel=Role.ADMIN)
    : base()
    {
        if (!typeof(IController).IsAssignableFrom(controllerType))
        {
            throw new ArgumentException($"Invalid controller type passed.\nType passed: {controllerType}");
        }
        this.endpointPattern = routePattern ?? null;
        this.HttpMethod = HttpMethod;
        this.controllerMethod = controllerMethod;
        this.controllerType = controllerType;
        this.routeTemplate = RouteTemplate;
        this.AccessLevel = accessLevel;
        this.Exists = true;
    }




    // OBSOLETE
    public void Deconstruct(out string routeTemplate, out HTTPMethod httpMethod, out Type controllerType, out MethodInfo controllerMethod, out Role accessLevel)
    {
        routeTemplate = RouteTemplate;
        httpMethod = HttpMethod;
        controllerType = ControllerType;
        controllerMethod = ControllerMethod;
        accessLevel = AccessLevel;
    }

    new public bool Equals(Object obj)
    {
        if (obj == null) return false;
        Endpoint endpoint = (Endpoint)obj;

        return
            HttpMethod == (HTTPMethod)endpoint.HttpMethod &&
            RouteTemplate == (string)endpoint.RouteTemplate &&
            ControllerType == (Type)endpoint.ControllerType &&
            AccessLevel == (Role)endpoint.AccessLevel &&
            ControllerMethod == (MethodInfo)endpoint.ControllerMethod;
    }

    public static bool operator ==(Endpoint e1, Endpoint e2)
    {
        if (e1 == null || e2 == null) return false;

        return
            e1.HttpMethod == (HTTPMethod)e2.HttpMethod &&
            e1.RouteTemplate == (string)e2.RouteTemplate &&
            e1.ControllerType == (Type)e2.ControllerType &&
            e1.AccessLevel == (Role)e2.AccessLevel &&
            e1.ControllerMethod == (MethodInfo)e2.ControllerMethod;
    }

    public static bool operator !=(Endpoint e1, Endpoint e2)
    {
        if (e1 == null && e2 == null) return false;
        else if (e1 == null || e2 == null) return true;

        return
            e1.HttpMethod != (HTTPMethod)e2.HttpMethod ||
            e1.RouteTemplate != (string)e2.RouteTemplate ||
            e1.ControllerType != (Type)e2.ControllerType ||
            e1.AccessLevel == (Role)e2.AccessLevel ||
            e1.ControllerMethod != (MethodInfo)e2.ControllerMethod;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(HttpMethod, RouteTemplate, ControllerType, ControllerMethod);
    }
    public IUrlParams UrlParams { get; set; }

    protected string? endpointPattern;

    protected MethodInfo controllerMethod;

    protected Type controllerType;
    private string routeTemplate;

    public string RouteTemplate => routeTemplate;

    public MethodInfo ControllerMethod => controllerMethod;

    public Type ControllerType => controllerType;

    public HTTPMethod HttpMethod { get; set; }

    public string EndpointPattern => endpointPattern;
    public Role AccessLevel { get; set; } = Role.ADMIN;
    public bool Exists { get; protected set; } = true;
}