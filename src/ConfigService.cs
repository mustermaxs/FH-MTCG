using System.Text.Json.Serialization;
using Npgsql.Replication;
using System.Collections.Generic;
using System.Text.Json;
using System.Collections.Concurrent;


namespace MTCG
{
    public interface IService
    {
    }

    public class ServiceProvider
    {
        protected ConcurrentDictionary<string, IService> localServices = new ConcurrentDictionary<string, IService>();
        protected static ConcurrentDictionary<string, IService> staticServices = new ConcurrentDictionary<string, IService>();
        protected static ConcurrentDictionary<string, Type> disposableServices = new ConcurrentDictionary<string, Type>();


        public virtual ServiceProvider Register<T>() where T : IService, new()
        {
            var serviceName = typeof(T).Name;
            var service = new T();

            localServices[serviceName] = service;
            Console.WriteLine($"[Registered Service] {serviceName}");

            return this;
        }
        public virtual ServiceProvider Register(IService service)
        {
            var serviceName = service.GetType().Name;

            localServices[serviceName] = service;
            Console.WriteLine($"[Registered Service] {serviceName}");

            return this;
        }


        public ServiceProvider RegisterStatic(IService service)
        {
            var serviceName = service.GetType().Name;

            staticServices[serviceName] = service;
            Console.WriteLine($"[Registered Service] {serviceName}");

            return this;
        }


        public ServiceProvider RegisterDisposable<T>() where T : IService, new()
        {
            var serviceName = typeof(T).Name;

            disposableServices[serviceName] = typeof(T);
            Console.WriteLine($"[Registered Service] {serviceName}");

            return this;
        }



        // public virtual ServiceProvider RegisterLocalService<T>() where T : IService
        // {
        //     var serviceName = typeof(T).Name;

        //     localServices[serviceName] = typeof(T);
        //     Console.WriteLine($"[Registered Service] {serviceName}");

        //     return this;
        // }

        // public virtual ServiceProvider RegisterLocalService(IService service)
        // {
        //     var serviceName = service.GetType().Name;

        //     localServices[serviceName] = service.GetType();
        //     Console.WriteLine($"[Registered Service] {serviceName}");

        //     return this;
        // }

        public ServiceProvider RegisterStatic<T>() where T : IService, new()
        {
            var serviceName = typeof(T).Name;
            var service = new T();

            staticServices[serviceName] = service;
            Console.WriteLine($"[Registered Service] {serviceName}");

            return this;
        }



        public bool Unregister<T>() where T : IService
        {
            return localServices.TryRemove(typeof(T).Name, out IService? _);
        }

        public virtual T Get<T>() where T : IService
        {
            Type serviceType = typeof(T);

            if (localServices.TryGetValue(serviceType.Name, out IService? config))
                return (T)config;

            throw new Exception($"Failed to get config {serviceType.Name}");
        }

        public static T GetDisposable<T>() where T : IService
        {
            Type serviceType = typeof(T);

            if (disposableServices.TryGetValue(serviceType.Name, out Type? service))
            {
                try
                {
                    T? serviceInstance = (T)Activator.CreateInstance(service);

                    if (serviceInstance == null)
                        throw new Exception($"Failed to create disposable service of type {serviceType.Name}");
                    
                    return serviceInstance;
                }
                catch (Exception ex)
                {
                    throw new Exception($"Failed to create disposable service of type {serviceType.Name}. Error: {ex.Message}", ex);
                }
            }

            throw new Exception($"Failed to get disposable service of type {serviceType.Name}");
        }



        public static T GetStatic<T>() where T : IService
        {
            Type serviceType = typeof(T);

            if (staticServices.TryGetValue(serviceType.Name, out IService? config))
                return (T)config;

            throw new Exception($"Failed to get config {serviceType.Name}");
        }

        // }
        // public class ConfigService : IServiceProvider
        // {
        //     private static ConfigService? configService = null;
        //     private IConfigLoader configLoader = new JsonConfigLoader();
        //     private IConfigLoader? customConfigLoader = null;

        //     //////////////////////////////////////////////////////////////////////
        //     //////////////////////////////////////////////////////////////////////

        //     [Obsolete("")]
        //     public void SetConfigLoader(IConfigLoader loader) => this.customConfigLoader = loader;



        //     public ConfigService()
        //     {
        //     }

        //     //////////////////////////////////////////////////////////////////////
        //     //////////////////////////////////////////////////////////////////////

        //     public override IServiceProvider Register<T>() where T : IConfig
        //     {
        //         var filePath = new T().FilePath;

        //         var config = (T)configLoader.LoadConfig<T>(filePath, new T().Section);

        //         if (config == default)
        //             throw new Exception("Failed to deserialize config file");

        //         services[config.Name] = config;
        //         Console.WriteLine($"[Registered Config] {config.Name}");
        //         return this;
        //     }




        //     //////////////////////////////////////////////////////////////////////
        //     //////////////////////////////////////////////////////////////////////

        //     /// <summary>
        //     /// Registers a config object.
        //     /// </summary>
        //     /// <param name="config">IConfig object.</param>
        //     /// <returns>ConfigService instance. For method chaining.</returns>
        //     public ConfigService Register(IConfig config)
        //     {
        //         if (ConfigService.configs.ContainsKey(config.Name))
        //         {
        //             Console.WriteLine($"ConfigService already contains config {config.Name}.");

        //             return this;
        //         }

        //         ConfigService.configs.Add(config.Name, config);
        //         Console.WriteLine($"Registered config {config.Name}");

        //         return this;
        //     }
        // }
    }
}

