using System;
using System.Collections;

namespace MTCG;

public interface IRouteObtainer
{
    public List<EndpointConfig> ObtainRoutes();
}

/// string routeTemplate, HTTPMethod, 