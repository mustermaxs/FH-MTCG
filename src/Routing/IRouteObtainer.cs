using System;
using System.Collections;

namespace MTCG;

public interface IRouteObtainer
{
    public List<Endpoint> ObtainRoutes();
}

/// string routeTemplate, HTTPMethod, 