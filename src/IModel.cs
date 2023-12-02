using System;
using System.Text.Json;

namespace MTCG;

public abstract class IModel<T>
{
    private string? message;
    public string? Message { get => message ?? ""; set => message = value; }
    virtual public string ToJsonString()
    {
        return JsonSerializer.Serialize(this);
    }
}
