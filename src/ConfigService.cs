using Newtonsoft.Json;
using Npgsql.Replication;


namespace MTCG
{
    public class ConfigService
    {
        private static ConfigService? configService = null;
        private static Dictionary<string, IConfig> configs = new Dictionary<string, IConfig>();

        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////


        private ConfigService()
        {
        }

        public static ConfigService GetInstance()
        {
            if (configService == null)
                configService = new ConfigService();

            return configService;
        }


        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////

        public ConfigService Register<T>(string? path) where T : IConfig, new()
        {
            var filePath = path ?? new T().FilePath;
            Dictionary<string, dynamic> completeConfig = FileHandler.ReadJsonFromFile(filePath) ?? throw new Exception("Failed to read config file");

            if (ConfigService.TryGetRelevantSection<T>(completeConfig, out T config))
                ConfigService.configs[config.Name] = config;
            else
                throw new Exception($"Failed to get relevant section for config {typeof(T).Name}");

            if (config == null) throw new Exception("Failed to deserialize config file");

            ConfigService.configs[config.Name] = config;
            Console.WriteLine($"Registered config {config.Name}");

            return this;
        }


        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////

        private static bool TryGetRelevantSection<T>(Dictionary<string, dynamic> completeConfig, out T config) where T : IConfig, new()
        {
            var sectionKey = new T().Section;

            if (!completeConfig.ContainsKey(sectionKey))
            {
                config = default!;
                return false;
            }

            var relevantSection = completeConfig[sectionKey];

            config = JsonConvert.DeserializeObject<T>(relevantSection.ToString()) ?? default!;

            return config != default;
        }


        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////


        public ConfigService Register(IConfig config)
        {
            if (ConfigService.configs.ContainsKey(config.Name))
            {
                Console.WriteLine("ConfigService already contains config.");

                return this;
            }

            ConfigService.configs.Add(config.Name, config);
            Console.WriteLine($"Registered config {config.Name}");

            return this;
        }


        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////


        public static bool Unregister(string name)
        {
            return ConfigService.configs.Remove(name);
        }


        public static bool Unregister<T>() where T : IConfig
        {
            Type configType = typeof(T);

            bool removedService = ConfigService.configs.Remove(configType.Name);

            if (removedService)
                Console.WriteLine($"Unregistered config {configType.Name}");
            else
                Console.WriteLine($"Failed to unregister config {configType.Name}");

            return removedService;
        }

        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////


        public static IConfig Get<T>() where T : IConfig
        {
            Type configType = typeof(T);

            if (ConfigService.configs.TryGetValue(configType.Name, out IConfig config))
                return config;

            throw new Exception($"Failed to get config {configType.Name}");
        }
    }
}

