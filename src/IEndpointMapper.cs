using System;
using System.Reflection;

namespace MTCG;

public interface IEndpointMapper
{
    public void RegisterEndpoint(string routePattern, HTTPMethod method, Type controllerType, MethodInfo controllerMethodName);
    public bool IsRouteRegistered(string routeTemplate, HTTPMethod method);
    public TokenizedUrl? MapRequestToEndpoint(string requestedUrl, HTTPMethod method);
}