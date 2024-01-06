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
            object? completeConfig = FileHandler.ReadJsonFromFile(filePath) ?? throw new Exception("Failed to read config file");

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

        private static bool TryGetRelevantSection<T>(dynamic completeConfig, out T config) where T : IConfig, new()
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

        private bool IsSubSection(string section)
        {
            return section.Contains("/");
        }

        public Dictionary<string, dynamic> GetSubSection(dynamic completeConfig, string searchedSection)
        {
            var sections = searchedSection.Split("/");
            var currentSection = completeConfig[sections[0]];

            for (int i = 1; i < sections.Length; i++)
            {
                if (!currentSection.ContainsKey(sections[i])) throw new Exception($"Failed to get subsection {searchedSection}");

                currentSection = currentSection[sections[i]];
            }

            return currentSection;
        }


        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Registers a config object.
        /// </summary>
        /// <param name="config">IConfig object.</param>
        /// <returns>ConfigService instance. For method chaining.</returns>
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

        /// <summary>
        /// Unregisters a config object.
        /// </summary>
        /// <param name="name">name of config object.</param>
        /// <returns>Boolean. True on success, false if config object isn't registered.</returns>
        public static bool Unregister(string name)
        {
            return ConfigService.configs.Remove(name);
        }

        /// <summary>
        /// Unregisters a config object.
        /// </summary>
        /// <param name="name">name of config object.</param>
        /// <returns>Boolean. True on success, false if config object isn't registered.</returns>
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


        /// <summary>
        /// Returns a config object.
        /// </summary>
        /// <typeparam name="T">Type of config object.</typeparam>
        /// <returns>T Config object</returns>
        /// <exception cref="Exception">Exception. If config object isn't registered.</exception>
        public static IConfig Get<T>() where T : IConfig
        {
            Type configType = typeof(T);

            if (ConfigService.configs.TryGetValue(configType.Name, out IConfig config))
                return config;

            throw new Exception($"Failed to get config {configType.Name}");
        }
    }
}

