using System;
using System.Text.Json;

namespace MTCG;

public static class Model
{
    public static T Create<T>(string jsonString)
    {
        return JsonSerializer.Deserialize<T>(jsonString);
    }
    public static string ToJsonString(IModel model) => JsonSerializer.Serialize(model);
}