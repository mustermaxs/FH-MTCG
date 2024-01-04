using System;
using System.Text.Json;

namespace MTCG;

public interface IModel
{
    // public static T Create<T>(string jsonString)
    // {
    //     return JsonSerializer.Deserialize<T>(jsonString);
    // }
    // public static string ToJsonString(IModel model) => JsonSerializer.Serialize(model);

    /// <summary>
    /// Returns a serializable object.
    /// Exposes only the fields that are required
    /// and supposed to be seen by the client.
    /// </summary>
    /// <returns>Object.</returns>
    public object ToSerializableObj();
}