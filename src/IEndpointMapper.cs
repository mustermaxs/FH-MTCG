using System;
using System.Reflection;

namespace MTCG;

public interface IEndpointMapper
{
    public void RegisterEndpoint(string routePattern, HTTPMethod method, Type controllerType, MethodInfo controllerMethodName, Permission accessLevel);
    public void RegisterEndpoint(IEndpoint endpoint);
    public bool IsRouteRegistered(string routeTemplate, HTTPMethod method);
    /// 12.02.2023 17:15
    /// TODO rename MapRequestToEndpoint
    /// returned routing context
    public IEndpoint? MapRequestToEndpoint(string requestedUrl, HTTPMethod method);
    public void MapRequestToEndpoint(ref IRequest context);
}