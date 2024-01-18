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
        protected static ConcurrentDictionary<string, Type> typeMappings = new ConcurrentDictionary<string, Type>();


        public virtual ServiceProvider RegisterLocal<T>() where T : IService, new()
        {
            var serviceName = typeof(T).Name;
            var service = new T();

            localServices[serviceName] = service;
            Console.WriteLine($"[Registered Service] {serviceName}");

            return this;
        }
        public virtual ServiceProvider RegisterLocal(IService service)
        {
            var serviceName = service.GetType().Name;

            localServices[serviceName] = service;
            Console.WriteLine($"[Registered Service]".PadRight(28) + $"{serviceName}");

            return this;
        }
        public virtual ServiceProvider RegisterPreconfigured(IService service)
        {
            var serviceName = service.GetType().Name;

            staticServices[serviceName] = service;
            Console.WriteLine($"[Registered Service]".PadRight(28) + $"{serviceName}");

            return this;
        }


        public ServiceProvider RegisterStatic(IService service)
        {
            var serviceName = service.GetType().Name;

            staticServices[serviceName] = service;
            Console.WriteLine($"[Registered Service]".PadRight(28) + $"{serviceName}");

            return this;
        }


        public ServiceProvider RegisterType<T>() where T : IService, new()
        {
            var serviceName = typeof(T).Name;

            typeMappings[serviceName] = typeof(T);
            Console.WriteLine($"[Registered Service]".PadRight(28) + $"{serviceName}");

            return this;
        }


        public ServiceProvider RegisterStatic<T>() where T : IService, new()
        {
            var serviceName = typeof(T).Name;
            var service = new T();

            staticServices[serviceName] = service;
            Console.WriteLine($"[Registered Service]".PadRight(28) + $"{serviceName}");

            return this;
        }



        public bool Unregister<T>() where T : IService
        {
            return localServices.TryRemove(typeof(T).Name, out IService? _);
        }

        public virtual T GetLocal<T>() where T : IService
        {
            Type serviceType = typeof(T);

            if (localServices.TryGetValue(serviceType.Name, out IService? config))
                return (T)config;

            throw new Exception($"Failed to get config {serviceType.Name}");
        }

        public static T GetFreshInstance<T>() where T : IService
        {
            Type serviceType = typeof(T);

            if (typeMappings.TryGetValue(serviceType.Name, out Type? service))
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



        public static T Get<T>() where T : IService
        {
            Type serviceType = typeof(T);

            if (staticServices.TryGetValue(serviceType.Name, out IService? config))
                return (T)config;

            throw new Exception($"Failed to get config {serviceType.Name}");
        }
    }
}

