using System;
using System.Collections;

namespace MTCG;

interface IRouteObtainer
{
    public List<object> ObtainRoutes();
}

/// string routeTemplate, HTTPMethod, 