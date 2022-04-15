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
                        staticCachedSystems.Add(type, new List<ISystem>());
                    }
                }
            }
        }

        private static readonly Dictionary<Type, List<ISystem>> staticCachedSystems = new();

        public event Action OnRegistryChange;
        private readonly Dictionary<Type, List<ISystem>> systems;

        public SystemsRegistry()
        {
            systems = staticCachedSystems.ToDictionary(i => i.Key, i => i.Value.ToList());
        }

        public IReadOnlyList<ISystem> SystemsOf(Type type) => systems[type];

        public void Add<T>(T system) where T : ISystem
        {
            var type = system.GetType();
            systems[type].Add(system);
            OnRegistryChange?.Invoke();
        }

        public void Remove<T>(T system) where T : ISystem
        {
            var type = system.GetType();
            systems[type].Remove(system);
            OnRegistryChange?.Invoke();
        }
    }
}