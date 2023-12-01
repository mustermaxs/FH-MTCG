using System;
using System.Reflection;

namespace MTCG;

class Endpoint : IEndpoint
{
    public Endpoint(HTTPMethod method, string routePattern, string routeTemplate, Type controllerType, MethodInfo controllerMethod)
    : base(method, routePattern, routeTemplate ,controllerType, controllerMethod)
    {
    }

}