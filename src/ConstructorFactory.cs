using System;
using System.Reflection;
using System.Reflection.Metadata;
using Npgsql.Replication;

namespace MTCG;

public delegate T InstanceFactory<T>();

public class ControllerManager
{
    public readonly Assembly assembly;
    public ControllerManager(Assembly assembly)
    {
        this.assembly = assembly ?? Assembly.GetExecutingAssembly();
    }
    public IEnumerable<Type> GetControllerTypes()
    {
        return assembly.GetTypes().Where(t => t.GetCustomAttribute<ControllerAttribute>() != null);
    }
    public IEnumerable<MethodInfo> GetRegisteredActionsForController(Type controllerType)
    {
        return controllerType.GetMethods()
        .Where(method => method.GetCustomAttribute<RouteAttribute>() != null);
    }
    public List<MethodInfo> GetMethodsWithAttributes<T>(Type classType) where T : Attribute
    {
        var methods = classType.GetMethods()
            .Where(method => method.GetCustomAttributes(typeof(T), true).Any())
            .ToList();

        return methods;
    }
    public RouteAttribute? GetRouteAttribute(MethodInfo methodInfo)
    {
        return (RouteAttribute)Attribute.GetCustomAttribute(methodInfo, typeof(RouteAttribute));
    }
}