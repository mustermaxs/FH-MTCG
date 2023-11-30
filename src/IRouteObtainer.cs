using System;
using System.Collections;

namespace MTCG;

interface IRouteObtainer
{
    public List<T> ObtainRoutes<T>();
}

/// string routeTemplate, HTTPMethod, 