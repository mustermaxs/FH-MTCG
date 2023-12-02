using System;
using System.Reflection;

namespace MTCG;

public class Endpoint : IEndpoint
{
    public Endpoint(HTTPMethod httpMethod, string? routePattern, string routeTemplate, Type controllerType, MethodInfo controllerMethod)
    : base(httpMethod, routePattern, routeTemplate ,controllerType, controllerMethod)
    {
    }

}