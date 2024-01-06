using System;
using MTCG;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks.Dataflow;

public class BetterJsonConfigLoader : IConfigLoader
{
    public T? LoadConfig<T>(string filePath, string? section) where T : IConfig, new()
    {
        var completeConfig = FileHandler.ReadJsonFromFile(filePath);

        JObject jObject = JObject.Parse(completeConfig!.ToString());

        if (IsSubSection(section))
        {
            var sections = GetSubsectionKeys(section);
            
            foreach (var s in sections)
            {
                if (jObject.TryGetValue(s, out var value))
                {
                    jObject = (JObject)value;
                }
                else
                {
                    throw new Exception("Failed to get relevant section for config");
                }
                
            }
        }

        return jObject.ToObject<T>();
    }

    private static bool IsSubSection(string section)
    {
        return section.Contains("/");
    }

    public static string[] GetSubsectionKeys(string section)
    {
        return section.Split("/");
    }
}