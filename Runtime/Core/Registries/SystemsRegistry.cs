using System;
using System.Collections.Generic;
using System.Linq;

namespace andywiecko.PBD2D.Core
{
    public class SystemsRegistry
    {
        static SystemsRegistry()
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (typeof(ISystem).IsAssignableFrom(type) && !type.IsAbstract)
                    {
                        staticCachedSystems.Add(type, default);
                    }
                }
            }
        }

        private static readonly Dictionary<Type, ISystem> staticCachedSystems = new();

        public event Action OnRegistryChange;
        private readonly Dictionary<Type, ISystem> systems;

        public SystemsRegistry()
        {
            systems = staticCachedSystems.ToDictionary(i => i.Key, i => default(ISystem));
        }

        public ISystem SystemOf(Type type) => systems[type];

        public void Add<T>(T system) where T : ISystem
        {
            var type = system.GetType();
            //TODO Add warning if system is already present
            systems[type] = system;
            OnRegistryChange?.Invoke();
        }

        public void Remove<T>(T system) where T : ISystem
        {
            var type = system.GetType();
            systems[type] = default;
            OnRegistryChange?.Invoke();
        }
    }
}