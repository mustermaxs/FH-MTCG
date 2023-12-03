using System;
using System.Text.Json;

namespace MTCG;

public class ResponseHandler
{
    private ResponseHandler() { }

    public static IResponse Create<T>(int statusCode, T payload, string text) where T : class
    {
        string stringifiedPayload = JsonSerializer.Serialize<T>(payload);

        return new Response(statusCode, stringifiedPayload, text);
    }
}