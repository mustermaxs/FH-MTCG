using System;
using System.Collections.Generic;
using System.Reflection;

namespace MTCG;

/// 12.04.2023 18:01
/// RENAME

/// keep track of number of parameters for constructor
/// also if there are multiple constructors
/// try to match the one that works with the provided parameters
/// if parameters don't fit => throw error
public interface IReflectionFactory<TClass> : IFactory<TClass>
{
    void ObtainClassesWithAttribute<TAttribute>();
    void GetConstructors();
    void GetConstructorParams();
}