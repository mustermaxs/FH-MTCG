using System;

namespace MTCG;

public interface IEndpointMapper
{
    public void RegisterEndpoint(string routePattern, HTTPMethod method, Type controllerType, string controllerMethodName);
    public bool IsRouteAlreadyRegistered(string route, HTTPMethod method);
    public ResolvedUrl? TryMapRouteToEndpoint(string requestedUrl, HTTPMethod method);
}