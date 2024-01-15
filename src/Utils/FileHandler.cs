using System;
using System.Diagnostics;
using Newtonsoft.Json;


namespace MTCG
{
    public static class FileHandler
    {

        /// <summary>
        /// Reads file and returns content as string[].
        /// </summary>
        /// <param name="path">File path.</param>
        /// <returns>string[]. Content of the file</returns>
        /// <exception cref="FileNotFoundException"></exception>
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


        /// <summary>
        /// Reads JSON file.
        /// </summary>
        /// <param name="path">File path.</param>
        /// <returns>Dynamic.</returns>
        public static dynamic? ReadJsonFromFile(string path)
        {
            using (StreamReader file = File.OpenText(path))
            using (JsonTextReader reader = new JsonTextReader(file))
            {
                JsonSerializer serializer = new JsonSerializer();

                return serializer.Deserialize<dynamic>(reader) ?? null;
            }
        }


        /// <summary>
        /// Reads JSON file and deserializes it to a given type.
        /// </summary>
        /// <typeparam name="T">Type to deserialize content to.</typeparam>
        /// <param name="path">File path.</param>
        /// <returns>T</returns>
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

