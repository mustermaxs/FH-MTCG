using System.Text.Json.Serialization;
using Npgsql.Replication;
using System.Collections.Generic;
using System.Text.Json;


namespace MTCG;

public class JsonConfigLoader : IConfigLoader
{
    public T? LoadConfig<T>(string filePath, string? section) where T : IConfig, new()
    {
        dynamic completeConfig = FileHandler.ReadJsonFromFile(filePath) ?? throw new Exception("Failed to read config file");

        if (TryGetRelevantSection<T>(completeConfig, out string config))
            return JsonSerializer.Deserialize<T>(config) ?? default!;
        else
            throw new Exception($"Failed to get relevant section for config {typeof(T).Name}");
    }

    private bool TryGetRelevantSection<T>(dynamic completeConfig, out string config) where T : IConfig, new()
    {
        var sectionString = new T().Section;
        var sectionKey = sectionString;

        if (IsSubSection(sectionString))
        {
            var sectionKeys = GetSubsectionKeys(sectionString);
            sectionKey = sectionKeys[sectionKeys.Length - 1];
            completeConfig = GetSubSection(completeConfig, sectionKeys);
        }

        if (!completeConfig.ContainsKey(sectionKey))
        {
            config = default!;
            return false;
        }

        var relevantSection = completeConfig[sectionKey];
        var relevantSectionString = relevantSection.ToString();

        config = relevantSectionString;

        return true;
    }

    private static bool IsSubSection(string section)
    {
        return section.Contains("/");
    }

    public static string[] GetSubsectionKeys(string section)
    {
        return section.Split("/");
    }

    public static dynamic GetSubSection(dynamic completeConfig, string[] sections)
    {
        var currentSection = completeConfig[sections[0]];

        for (int i = 1; i < sections.Length - 1; i++)
        {
            if (!currentSection.ContainsKey(sections[i])) throw new Exception($"Failed to get subsection {sections[i]}");

            currentSection = currentSection[sections[i]];
        }

        return currentSection;
    }
}