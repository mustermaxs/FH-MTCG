using System;
using System.Reflection;

namespace MTCG;

/// <summary>
/// Extension of System.Reflection MethodBase.Invoke
/// Used for passing the named parameters from URL to controller methods
/// in the right order
/// </summary>
public static class CustomReflectionExtension
{
    public static T MapArgumentsAndInvoke<T>(this MethodBase self, object classInstance, Dictionary<string, object> providedParams)
    {
        if (providedParams.Count == 0)
        {
            return (T)self.Invoke(classInstance, null);
        }
        else
        {
            return (T)self.Invoke(classInstance, MapProvidedArgumentsWithSignature(self, providedParams));
        }
    }

    public static object[] MapProvidedArgumentsWithSignature(MethodBase classInstanceMethod, Dictionary<string, object> providedParams)
    {
        string[] expectedParamNames = classInstanceMethod.GetParameters().Select(param => param.Name).ToArray();
        object[] parameters = new object[expectedParamNames.Length];

        for (int i = 0; i < parameters.Length; ++i)
        {
            parameters[i] = Type.Missing;
        }
        foreach (var param in providedParams)
        {
            var providedName = param.Key;
            var paramIndex = Array.IndexOf(expectedParamNames, providedName);

            if (paramIndex >= 0)
            {
                parameters[paramIndex] = param.Value;
            }
        }
        return parameters;
    }
}