using System;
using System.Collections.Generic;
using UnityEngine;

namespace andywiecko.PBD2D.Core
{
    public static class SystemsRegistry
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void Initialize()
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (typeof(ISystem).IsAssignableFrom(type) && !type.IsAbstract)
                    {
                        systems.Add(type, new List<ISystem>());
                    }
                }
            }
        }

        public static event Action OnRegistryChange;

        private static readonly Dictionary<Type, List<ISystem>> systems = new();

        public static void InvokeRegistryChange() => OnRegistryChange?.Invoke();

        public static IReadOnlyList<ISystem> SystemsOf(Type type) => systems[type];

        public static void Add<T>(T system) where T : ISystem
        {
            var type = system.GetType();
            systems[type].Add(system);
            InvokeRegistryChange();
        }

        public static void Remove<T>(T system) where T : ISystem
        {
            var type = system.GetType();
            systems[type].Remove(system);
            InvokeRegistryChange();
        }
    }
}