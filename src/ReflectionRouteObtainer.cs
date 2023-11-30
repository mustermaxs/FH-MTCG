using System;
using System.Reflection;

namespace MTCG;

class ReflectionRouteObtainer : IRouteObtainer
{
  private IAttributeHandler attributeHandler;

  public ReflectionRouteObtainer(IAttributeHandler attributeHandler)
    {
        this.attributeHandler = attributeHandler;
    }
    public List<object> ObtainRoutes()
    {
        var endpointList = new List<AbstractEndpoint>();

        var controllerTypes = attributeHandler.GetAttributeOfType<ControllerAttribute>(typeof(IController));
        
        foreach (var controllerType in controllerTypes)
        {
            var controllerMethodsInfos = attributeHandler.GetClassMethodsWithAttribute<RouteAttribute>(controllerType);

            foreach (MethodInfo methodInfo in controllerMethodsInfos)
            {
                var routeAttribute = attributeHandler.GetMethodAttributeWithMethodInfo<RouteAttribute>(methodInfo);
                var endpointConfig = new{
                    method=routeAttribute?.Method,
                    routeTemplate=routeAttribute?.RouteTemplate,
                    controllerType=controllerType,
                    controllerMethod=methodInfo.Name
                };
            }
        }
    }
}