using System;
using System.Reflection;

namespace MTCG;

public abstract class IAttributeHandler
{
    public readonly Assembly assembly;
    public IAttributeHandler(Assembly assembly)
    {
        this.assembly = assembly ?? Assembly.GetExecutingAssembly();
    }
    public IEnumerable<Type> GetAttributeOfType<T>(Type elementType) where T : Attribute
    {
        return assembly.GetTypes().Where(
            t => t.GetCustomAttribute(typeof(T), true) != null);
    }
    public IEnumerable<MethodInfo> GetMethodInfosOfClassWithAttribute<T>(Type classType) where T : Attribute
    {
        return (classType).GetMethods()
        .Where(
            method => method.GetCustomAttribute(typeof(T), true) != null);
    }
    public List<MethodInfo> GetClassMethodsWithAttribute<T>(Type classType) where T : Attribute
    {
        return classType.GetMethods()
            .Where(
                method => method.GetCustomAttributes(typeof(T), true).Any())
            .ToList();
    }
    public T? GetMethodAttributeWithMethodInfo<T>(MethodInfo methodInfo) where T : Attribute
    {
        return (T)Attribute.GetCustomAttribute(methodInfo, typeof(T));
    }
}