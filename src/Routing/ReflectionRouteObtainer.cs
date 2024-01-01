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
            ControllerMethod == (MethodInfo)endpointConfig.ControllerMethod);
        }
        catch (Exception ex)
        {
            return false;
        }

    }
    public HTTPMethod Method { get; set; }
    public string RouteTemplate { get; set; }
    public Type ControllerType { get; set; }
    public MethodInfo ControllerMethod { get; set; }

    public void Deconstruct(out HTTPMethod method, out string routeTemplate, out Type controllerType, out MethodInfo controllerMethod)
    {
        method = Method;
        routeTemplate = RouteTemplate;
        controllerType = ControllerType;
        controllerMethod = ControllerMethod;
    }
}




/// <summary>
/// Obtains valid endpoint definitions from controllers via custom attributes
/// (RouteAttribute, ControllerAttribute)
/// </summary>
public class ReflectionRouteObtainer : IRouteObtainer
{
    private IAttributeHandler attributeHandler;

    public ReflectionRouteObtainer(IAttributeHandler attributeHandler)
    {
        this.attributeHandler = attributeHandler;
    }

    /// <summary>
    /// Obtains the routes by using reflection to scan the controllers and their methods.
    /// </summary>
    /// <returns>A list of endpoints representing the obtained routes.</returns>
    public List<Endpoint> ObtainRoutes()
    {
        var endpointList = new List<Endpoint>();

        var controllerTypes = attributeHandler.GetAttributesOfType<ControllerAttribute>();

        foreach (var controllerType in controllerTypes)
        {
            var controllerMethodsInfos = attributeHandler.GetClassMethodsWithAttribute<RouteAttribute>(controllerType);

            foreach (var methodInfo in controllerMethodsInfos)
            {
                var routeAttribute = attributeHandler.GetMethodAttributeWithMethodInfo<RouteAttribute>(methodInfo);

                endpointList.Add(new Endpoint
                (
                    (HTTPMethod)routeAttribute!.Method,
                    null,
                    routeAttribute!.RouteTemplate,
                    (Type)controllerType,
                    (MethodInfo)methodInfo,
                    (Role)routeAttribute!.AccessLevel
                ));
            }
        }

        return endpointList;
    }
}
