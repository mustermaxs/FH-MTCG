using System;

namespace MTCG;

public interface IEndpointMapper
{
    public void RegisterEndpoint(string routePattern, HTTPMethod method);
    public bool IsRouteAlreadyRegistered(string routePattern, HTTPMethod method);
    public ResolvedUrl? TryMapRequestedRoute(string requestedUrl, HTTPMethod method);
}