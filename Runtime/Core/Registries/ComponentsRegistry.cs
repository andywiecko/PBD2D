using System;
using System.Collections.Generic;
using System.Reflection;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace andywiecko.PBD2D.Core
{
    public class ComponentsRegistry
    {
        private static readonly Dictionary<Type, IReadOnlyList<(MethodInfo Add, MethodInfo Remove)>> derivedTypesInterfacesMethods = new();
        private static readonly IReadOnlyList<Type> typesToCache = new[] { typeof(BaseComponent), typeof(ComponentsTuple), typeof(FreeComponent) };

#if UNITY_EDITOR
        [InitializeOnLoadMethod]
        private static void EditorInitialization() => Initialize();
#endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void Initialize()
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in assembly.GetTypes())
                {
                    foreach (var typeToCache in typesToCache)
                    {
                        if (typeToCache.IsAssignableFrom(type))
                        {
                            RegisterTypeInterfaces(type);
                        }
                    }
                }
            }
        }

        private static void RegisterTypeInterfaces(Type type)
        {
            var list = new List<(MethodInfo, MethodInfo)>();
            foreach (var @interface in type.GetInterfaces())
            {
                if (typeof(IComponent).IsAssignableFrom(@interface))
                {
                    list.Add(GetMethods(@interface));
                }
            }
            derivedTypesInterfacesMethods.TryAdd(type, list);

            static (MethodInfo, MethodInfo) GetMethods(Type type)
            {
                var registryType = typeof(ComponentsRegistry<>).MakeGenericType(type);
                var flags = BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy;
                return (registryType.GetMethod("Add", flags), registryType.GetMethod("Remove", flags));
            }
        }

        public static void Register(object instance)
        {
            var @this = new[] { instance };
            foreach (var (addMethod, _) in derivedTypesInterfacesMethods[instance.GetType()])
            {
                addMethod.Invoke(null, @this);
            }
        }

        public static void Deregister(object instance)
        {
            var @this = new[] { instance };
            foreach (var (_, removeMethod) in derivedTypesInterfacesMethods[instance.GetType()])
            {
                removeMethod.Invoke(null, @this);
            }
        }
    }

    public class ComponentsRegistry<T> : ComponentsRegistry
        where T : IComponent
    {
        public static event Action<T> OnAddComponent;
        public static event Action<T> OnRemoveComponent;
        private static readonly List<T> components = new ();
        public static IReadOnlyList<T> Components() => components;
        public static void Add(T component)
        {
            components.Add(component);
            OnAddComponent?.Invoke(component);
        }
        public static void Remove(T component)
        {
            components.Remove(component);
            OnRemoveComponent?.Invoke(component);
        }
    }
}