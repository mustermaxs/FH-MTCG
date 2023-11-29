using System;

namespace MTCG;

class Endpoint : AbstractEndpoint
{
    public Endpoint(HTTPMethod method, string routePattern, string routeTemplate, Type controllerType, string controllerMethod)
    : base(method, routePattern, routeTemplate ,controllerType, controllerMethod)
    {
    }

}