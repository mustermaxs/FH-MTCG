using System;
using System.Diagnostics;
using Newtonsoft.Json;


namespace MTCG
{
    public static class Constants
    {
        public const string CONFIG_FILE_PATH = "./config.txt";
    }

    public abstract class IConfig
    {
        public abstract string Name { get; }
    }

    public class ServerConfig : IConfig
    {
        public override string Name => "ServerConfig";
    }

    public class ConfigService
    {
        private static Dictionary<string, IConfig> configs = new Dictionary<string, IConfig>();

        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////


        public static void Register<T>(string path) where T : IConfig, new()
        {
            var config = FileHandler.ReadJsonFromFile<T>(path);

            if (config == null) throw new Exception("Failed to deserialize config file");

            ConfigService.configs[config.Name] = config;
        }


        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////


        public static void Register(IConfig config)
        {
            if (ConfigService.configs.ContainsKey(config.Name))
                return;

            ConfigService.configs.Add(config.Name, config);
        }


        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        
        
        public static bool Unregister(string name)
        {
            return ConfigService.configs.Remove(name);
        }

        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////


        public static IConfig? Get<T>() where T : IConfig
        {
            Type configType = typeof(T);

            if (ConfigService.configs.TryGetValue(configType.Name, out IConfig config))
                return config;

            return null;
        }
    }



    public static class FileHandler
    {
        public static string[] ReadFileAsStrings(string path)
        {
            if (!File.Exists(path)) throw new FileNotFoundException($"File {path} not found.");

            return File.ReadAllLines(path);
        }


        /// <summary>
        /// Searches for a key in a file and returns the value.
        /// Keys and values are separated by ';;'.
        /// </summary>
        /// <param name="path">Path to file.</param>
        /// <param name="key">Key to search for.</param>
        /// <returns>Value of key.</returns>
        public static string? SearchKeyValueInFile(string path, string key)
        {
            string[] lines = ReadFileAsStrings(path);

            return lines.FirstOrDefault(line => line.Split(";;")[0] == key)?.Split(";;")[1] ?? string.Empty;
        }
        public static Dictionary<string, dynamic>? ReadJsonFromFile(string path)
        {
            using (StreamReader file = File.OpenText(path))
            using (JsonTextReader reader = new JsonTextReader(file))
            {
                JsonSerializer serializer = new JsonSerializer();

                return serializer.Deserialize<Dictionary<string, dynamic>>(reader) ?? null;
            }
        }
        public static T? ReadJsonFromFile<T>(string path) where T : class, new()
        {
            using (StreamReader file = File.OpenText(path))
            using (JsonTextReader reader = new JsonTextReader(file))
            {
                JsonSerializer serializer = new JsonSerializer();

                return serializer.Deserialize<T>(reader) ?? null;
            }
        }

    }
}

