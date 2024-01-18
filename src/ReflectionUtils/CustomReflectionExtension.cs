using System;
using System.Collections.Generic;
using System.Reflection;

namespace MTCG;
public static class ReflectionUtils
{
    /// <summary>
    /// Maps keys from dictionary with signature of provided method-base and
    /// and passes the dictionarys values to the method.
    /// type conversion from gets handled automatically
    /// </summary>
    /// <param name="TReturnType">Type of return value</param>
    /// <param name="TValueType">Type of dictionary value</param>
    /// <param name="classInstance">Object whose method should get executed</param>
    /// <param name="providedParams">
    /// Arguments that should get mapped according to 
    /// signature of the invoked method
    /// </param>


    public static TReturnType MapArgumentsAndInvoke<TReturnType, TValueType>(
        this MethodBase self, object classInstance, Dictionary<string, TValueType> providedParams) where TReturnType :  IResponse
    {
        if (providedParams == null || providedParams.Count == 0)
        {
            return (TReturnType)self.Invoke(classInstance, null);
        }
        else
        {
            return (TReturnType)self.Invoke(
                classInstance,
                MapProvidedArgumentsToSignature<TValueType>(self, providedParams)); // CHANGED
        }
    }


    
    public static Task<TReturnType> MapArgumentsAndInvokeAsync<TReturnType, TValueType>(
        this MethodBase self, object classInstance, Dictionary<string, TValueType> providedParams)
    {
        if (providedParams == null || providedParams.Count == 0)
        {
            return (Task<TReturnType>)self.Invoke(classInstance, null);
        }
        else
        {
            return (Task<TReturnType>)self.Invoke(
                classInstance,
                MapProvidedArgumentsToSignature<TValueType>(self, providedParams)); // CHANGED
        }
    }


    public static TReturnType? MapArgumentsAndCreateInstance<TReturnType>(
        this ConstructorInfo self)
    {

            return (TReturnType)self.Invoke(null);

    }



    public static object[]? MapProvidedArgumentsToSignature<TValueType>(
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

    Dictionary<Type, Func<object, dynamic>> converters = new Dictionary<Type, Func<object, dynamic>>
    {
        { typeof(bool), ConvertToBoolean },
        { typeof(int), ConvertToInt },
        { typeof(float), ConvertToFloat },
        { typeof(Guid), ConvertToGuid },
    };

    if (converters.TryGetValue(toType, out var conversionFunc))
    {
        return conversionFunc(value);
    }

    throw new InvalidOperationException($"Cannot convert {value.GetType()} to {toType}");
}

private static dynamic ConvertToBoolean(object value)
{
    string stringValue = value!.ToString()?.Trim().ToLower();
    switch (stringValue)
    {
        case "true":
        case "yes":
        case "1":
            return true;
        case "false":
        case "no":
        case "0":
            return false;
        default:
            throw new InvalidOperationException($"Cannot convert {value.GetType()} to boolean");
    }
}

private static dynamic ConvertToInt(object value)
{
    if (Int32.TryParse(value!.ToString(), out int intVal))
    {
        return intVal;
    }
    else
    {
        throw new InvalidOperationException($"Cannot convert {value.GetType()} to int");
    }
}

private static dynamic ConvertToFloat(object value)
{
    if (float.TryParse(value!.ToString(), out float floatVal))
    {
        return floatVal;
    }
    else
    {
        throw new InvalidOperationException($"Cannot convert {value.GetType()} to float");
    }
}

private static dynamic ConvertToGuid(object value)
{
    if (Guid.TryParse(value!.ToString(), out Guid guidVal))
    {
        return guidVal;
    }
    else
    {
        throw new InvalidOperationException($"Cannot convert {value.GetType()} to Guid");
    }
}
}
