using System;
using System.Reflection;
using System.Reflection.Metadata;

namespace MTCG;

public class EndpointConfig
{
    public override bool Equals(Object obj)
    {
        if (obj == null) return false;

        try
        {
            EndpointConfig endpointConfig = (EndpointConfig)obj;
            return (Method == (HTTPMethod)endpointConfig.Method &&
            RouteTemplate == (string)endpointConfig.RouteTemplate &&
            ControllerType == (Type)endpointConfig.ControllerType &&
            ControllerMethod == (string)endpointConfig.ControllerMethod);
        }
        catch (Exception ex)
        {
            return false;
        }

    }
    public HTTPMethod Method { get; set; }
    public string RouteTemplate { get; set; }
    public Type ControllerType { get; set; }
    public string ControllerMethod { get; set; }

    public void Deconstruct(out HTTPMethod method, out string routeTemplate, out Type controllerType, out string controllerMethod)
    {
        method = Method;
        routeTemplate = RouteTemplate;
        controllerType = ControllerType;
        controllerMethod = ControllerMethod;
    }
}

/// <summary>
/// Obtains valid endpoint definitions via custom attributes
/// (RouteAttribute, ControllerAttribute)
/// </summary>


public class ReflectionRouteObtainer : IRouteObtainer
{
    private IAttributeHandler attributeHandler;

    public ReflectionRouteObtainer(IAttributeHandler attributeHandler)
    {
        this.attributeHandler = attributeHandler;
    }

    public List<EndpointConfig> ObtainRoutes()
    {
        var endpointList = new List<EndpointConfig>();

        var controllerTypes = attributeHandler.GetAttributeOfType<ControllerAttribute>(typeof(IController));

        foreach (var controllerType in controllerTypes)
        {
            var controllerMethodsInfos = attributeHandler.GetClassMethodsWithAttribute<RouteAttribute>(controllerType);

            foreach (var methodInfo in controllerMethodsInfos)
            {
                var routeAttribute = attributeHandler.GetMethodAttributeWithMethodInfo<RouteAttribute>(methodInfo);
                var endpointConfig = new EndpointConfig
                {
                    Method = (HTTPMethod)routeAttribute?.Method,
                    RouteTemplate = routeAttribute?.RouteTemplate,
                    ControllerType = (Type)controllerType,
                    ControllerMethod = methodInfo.Name
                };

                endpointList.Add(endpointConfig);
            }
        }

        return endpointList;
    }
}
