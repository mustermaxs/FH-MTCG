using System;
using System.Collections.Generic;
using System.Reflection;

namespace MTCG;
public static class CustomReflectionExtension
{
    /// <summary>
    /// Maps keys from dictionary with signature of provided method-base and
    /// and passes the dictionarys values to the method.
    /// type conversion from gets handled automatically
    /// </summary>
    
    public static TReturnType MapArgumentsAndInvoke<TReturnType, TValueType>(
        this MethodBase self, object classInstance, Dictionary<string, TValueType> providedParams)
    {
        if (providedParams == null || providedParams.Count == 0)
        {
            return (TReturnType)self.Invoke(classInstance, null);
        }
        else
        {
            return (TReturnType)self.Invoke(classInstance, MapProvidedArgumentsWithSignature(self, providedParams));
        }
    }

    public static object[]? MapProvidedArgumentsWithSignature<TValueType>(
        MethodBase classInstanceMethod, Dictionary<string, TValueType> providedParams)
    {
        // Type[] expectedParamTypes = classInstanceMethod.GetParameters().Select(param => param.ParameterType).ToArray();
        ParameterInfo[] expectedParams = classInstanceMethod.GetParameters().ToArray();

        if (expectedParams.Length == 0 || expectedParams == null)
        {
            return null;
        }

        string[] expectedParamNames = expectedParams.Select(param => param.Name!).ToArray();
        object[] parameters = new object[expectedParamNames.Length];

        for (int i = 0; i < parameters.Length; ++i)
        {
            parameters[i] = Type.Missing;
        }
        foreach (var param in providedParams)
        {
            var providedName = param.Key;
            var paramIndex = Array.IndexOf(expectedParamNames, providedName);
            Type expectedParamType = expectedParams[paramIndex].ParameterType;

            if (paramIndex >= 0)
            {
                parameters[paramIndex] = (dynamic)ConvertToType(param!.Value!, expectedParamType);
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
    public static dynamic ConvertToType(object value, Type toType)
    {
        if (value == null)
        {
            throw new ArgumentNullException(nameof(value));
        }

        if (toType.IsAssignableFrom(value.GetType()))
        {
            return Convert.ChangeType(value, toType);
        }

        try
        {
            if (toType == typeof(bool))
            {
                string stringValue = value!.ToString()?.Trim().ToLower();
                if (stringValue == "true" || stringValue == "yes" || stringValue == "1")
                {
                    return (bool)(object)true;
                }
                else if (stringValue == "false" || stringValue == "no" || stringValue == "0")
                {
                    return (bool)(object)false;
                }
            }

            return Convert.ChangeType(value, toType);
        }
        catch (InvalidCastException)
        {
            throw new InvalidOperationException($"Cannot convert {value.GetType()} to {toType}");
        }
    }
}
