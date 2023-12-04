// using System;
// using System.Collections.Generic;
// using System.Reflection;
// using System.Reflection.Metadata;

// namespace MTCG;

// public class ReflectionClassFactory<TClass> : IReflectionFactory<TClass>
// {
//     private IAttributeHandler attributeHandler;
//     private registeredClasses;
//     public ReflectionClassFactory(IAttributeHandler attributeHandler)
//     {
//         this.attributeHandler = attributeHandler;
//     }

//     public void GetConstructors()

//     public void ObtainClassesWithAttribute<TAttribute>() where TAttribute : Attribute
//     {
//         AttributeUsageAttribute? usage = attributeHandler.GetAttributeUsage<TAttribute>();

//         if (usage != null && (usage.ValidOn != AttributeTargets.Class))
//         {
//             throw new ArgumentException($"Attribute doesn't apply to class.");
//         }
        
//         var attributes = attributeHandler.GetAttributesOfType<TAttribute>();

//     }
// }