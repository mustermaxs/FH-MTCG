using System;
using System.Text.Json;

namespace MTCG;

public abstract class PayloadResponse<T> : IResponse where T : class?
{
    public T? Payload { get; }
    virtual public int StatusCode { get; }
    virtual public string Description { get; }

    public PayloadResponse(int statusCode, T? payload, string? description)
    {
        this.Payload = payload;
        this.StatusCode = statusCode;
        this.Description = description ?? string.Empty;
    }

    virtual public string PayloadAsJson()
    {
        if (Payload == null)
        {
            return string.Empty;
        }

        return JsonSerializer.Serialize<T>(Payload);
    }
}