using System;
using System.Text.Json;

namespace MTCG;

public class ResponseHandler
{
    private ResponseHandler() { }

    public static IResponse Create<T>(int statusCode, IEnumerable<T> payload, string text)
    {
        string stringifiedPayload = JsonSerializer.Serialize<IEnumerable<T>>(payload);
        return new Response(statusCode, stringifiedPayload, text);
    }

    public static IResponse Create<T>(int statusCode, T payload, string text)
    {
        string stringifiedPayload = JsonSerializer.Serialize<T>(payload);
        return new Response(statusCode, stringifiedPayload, text);
    }
}