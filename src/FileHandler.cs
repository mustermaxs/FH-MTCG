using System;
using System.Diagnostics;
using Newtonsoft.Json;


namespace MTCG
{
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

