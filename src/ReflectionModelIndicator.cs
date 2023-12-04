using System;
using System.Reflection;
using System.Reflection.Metadata;

namespace MTCG;

public class ReflectionModelIndicator
{
    private IAttributeHandler attributeHandler;

    public ReflectionModelIndicator(IAttributeHandler attributeHandler)
    {
        this.attributeHandler = attributeHandler;
    }

    public void GetModelAttributes<T>()
    {
        // var modelAttributes = attributeHandler.GetAttributeOfType<UseModelAttribute>(typeof(UseModelAttribute));
        
    }
}