using System;

namespace MTCG;

class Endpoint : AbstractEndpoint
{
    public Endpoint(HTTPMethod method, string routePattern, Type controllerType, string controllerMethod)
    : base(method, routePattern, controllerType, controllerMethod)
    {
    }
}