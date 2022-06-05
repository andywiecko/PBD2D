using System;
using System.Collections.Generic;
using System.Linq;

namespace andywiecko.PBD2D.Core
{
    public class ConfigurationsRegistry
    {
        static ConfigurationsRegistry()
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (typeof(IConfiguration).IsAssignableFrom(type) && !type.IsAbstract)
                    {
                        staticCachedConfigurations.Add(type, default);
                    }
                }
            }
        }

        private static readonly Dictionary<Type, IConfiguration> staticCachedConfigurations = new();

        public event Action OnRegistryChange;
        private readonly Dictionary<Type, IConfiguration> configs;

        public ConfigurationsRegistry()
        {
            configs = staticCachedConfigurations.ToDictionary(i => i.Key, i => default(IConfiguration));
        }

        public T Get<T>() where T : class, IConfiguration => configs[typeof(T)] as T;

        public void Set<T>(T config) where T : IConfiguration
        {
            //TODO Add warning if system is already present
            configs[typeof(T)] = config;
            OnRegistryChange?.Invoke();
        }
    }
}