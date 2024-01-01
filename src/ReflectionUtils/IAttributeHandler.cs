using System;
using System.ComponentModel;
using System.Reflection;

namespace MTCG;

public abstract class IAttributeHandler
{
    public readonly Assembly assembly;



    
    public IAttributeHandler(Assembly? assembly = null)
    {
        this.assembly = assembly ?? Assembly.GetExecutingAssembly();
    }




    virtual public AttributeUsageAttribute? GetAttributeUsage<TAttribute>() where TAttribute : Attribute
    {
        return Attribute.GetCustomAttribute(typeof(TAttribute), typeof(AttributeUsageAttribute)) as AttributeUsageAttribute;
    }



    /// 12.03.2023 22:29
    /// ? argument notwendig
    virtual public IEnumerable<Type> GetAttributesOfType<T>() where T : Attribute
    {
        return assembly.GetTypes().Where(
            t => t.GetCustomAttribute(typeof(T), true) != null);
    }




    /// 12.03.2023 22:29
    /// ? argument notwendig
    virtual public IEnumerable<MethodInfo> GetMethodInfosOfClassWithAttribute<T>(Type classType) where T : Attribute
    {
        return (classType).GetMethods()
        .Where(
            method => method.GetCustomAttribute(typeof(T), true) != null);
    }




    /// 12.03.2023 22:29
    /// ? Argument notwendig
    virtual public List<MethodInfo> GetClassMethodsWithAttribute<T>(Type classType) where T : Attribute
    {
        return classType.GetMethods()
            .Where(
                method => method.GetCustomAttributes(typeof(T), true).Any())
            .ToList();
    }




    virtual public T? GetMethodAttributeWithMethodInfo<T>(MethodInfo methodInfo) where T : Attribute
    {
        return (T)Attribute.GetCustomAttribute(methodInfo, typeof(T));
    }
}