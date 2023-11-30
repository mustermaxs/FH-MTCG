using System;

namespace MTCG;

public interface IEndpointMapper
{
    public void RegisterEndpoint(string routePattern, HTTPMethod method, Type controllerType, string controllerMethodName);
    public bool IsRouteRegistered(string routeTemplate, HTTPMethod method);
    public RequestObject? TryMapRouteToEndpoint(string requestedUrl, HTTPMethod method);
}