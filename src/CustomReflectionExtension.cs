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
    public static TReturnType MapArgumentsAndInvoke<TReturnType, TValueType>(this MethodBase self, object classInstance, Dictionary<string, TValueType> providedParams)
    {
        if (providedParams.Count == 0)
        {
            return (TReturnType)self.Invoke(classInstance, null);
        }
        else
        {
            return (TReturnType)self.Invoke(classInstance, MapProvidedArgumentsWithSignature(self, providedParams));
        }
    }

    public static object[]? MapProvidedArgumentsWithSignature<TValueType>(MethodBase classInstanceMethod, Dictionary<string, TValueType> providedParams)
    {
        Type[] expectedParams = classInstanceMethod.GetParameters().Select(param => param.ParameterType).ToArray();

        if (expectedParams.Length == 0 || expectedParams == null)
        {
            return null;
        }

        string[] expectedParamNames = classInstanceMethod.GetParameters().Select(param => param.Name!).ToArray();
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

    public static TExpectedType ConvertToType<TExpectedType>(object value)
    {
        if (value == null)
        {
            throw new ArgumentNullException(nameof(value));
        }

        if (typeof(TExpectedType).IsAssignableFrom(value.GetType()))
        {
            return (TExpectedType)value;
        }

        try
        {
            if (typeof(TExpectedType) == typeof(bool))
            {
                string stringValue = value!.ToString()?.Trim().ToLower();
                if (stringValue == "true" || stringValue == "yes" || stringValue == "1")
                {
                    return (TExpectedType)(object)true;
                }
                else if (stringValue == "false" || stringValue == "no" || stringValue == "0")
                {
                    return (TExpectedType)(object)false;
                }
            }

            return (TExpectedType)Convert.ChangeType(value, typeof(TExpectedType));
        }
        catch (InvalidCastException)
        {
            throw new InvalidOperationException($"Cannot convert {value.GetType()} to {typeof(TExpectedType)}");
        }
    }


}