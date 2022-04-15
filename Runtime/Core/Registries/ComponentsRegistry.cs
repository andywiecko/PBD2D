using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace andywiecko.PBD2D.Core
{
    public class ComponentsRegistry
    {
        private static readonly Dictionary<Type, IReadOnlyList<Type>> derivedTypesToInterfaces = new();
        private static readonly HashSet<Type> staticComponentInterfaces = new();

        static ComponentsRegistry()
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in assembly.GetTypes())
                {
                    RegisterTypeInterfaces(type);
                }
            }
        }

        private static void RegisterTypeInterfaces(Type type)
        {
            var list = new List<Type>();
            foreach (var @interface in type.GetInterfaces())
            {
                if (typeof(IComponent).IsAssignableFrom(@interface))
                {
                    list.Add(@interface);
                    staticComponentInterfaces.Add(@interface);
                }
            }
            derivedTypesToInterfaces.TryAdd(type, list);
        }

        private readonly Dictionary<Type, IList> components = new();
        private readonly Dictionary<Type, Action<object>> onAddActions = new();
        private readonly Dictionary<Type, Action<object>> onRemoveActions = new();

        public ComponentsRegistry()
        {
            components = staticComponentInterfaces.ToDictionary(i => i, i => CreateListOf(i));
            onAddActions = staticComponentInterfaces.ToDictionary(i => i, i => default(Action<object>));
            onRemoveActions = staticComponentInterfaces.ToDictionary(i => i, i => default(Action<object>));

            static IList CreateListOf(Type t) => Activator.CreateInstance(typeof(List<>).MakeGenericType(t)) as IList;
        }

        public IReadOnlyList<T> GetComponents<T>() where T : IComponent => components[typeof(T)] as List<T>;
        public IEnumerable GetComponents(Type type) => components[type];

        public void Add(object instance)
        {
            var type = instance.GetType();
            foreach (var @interface in derivedTypesToInterfaces[type])
            {
                components[@interface].Add(instance);
                onAddActions[@interface]?.Invoke(instance);
            }
        }

        public void Remove(object instance)
        {
            var type = instance.GetType();
            foreach (var @interface in derivedTypesToInterfaces[type])
            {
                components[@interface].Remove(instance);
                onRemoveActions[@interface]?.Invoke(instance);
            }
        }

        public void SubscribeOnAdd<T>(Action<object> fun) where T : IComponent => SubscribeOnAdd(typeof(T), fun);
        public void UnsubscribeOnAdd<T>(Action<object> fun) where T : IComponent => UnsubscribeOnAdd(typeof(T), fun);
        public void SubscribeOnAdd(Type type, Action<object> fun) => onAddActions[type] += fun;
        public void UnsubscribeOnAdd(Type type, Action<object> fun) => onAddActions[type] -= fun;
        public void SubscribeOnRemove<T>(Action<object> fun) where T : IComponent => SubscribeOnRemove(typeof(T), fun);
        public void UnsubscribeOnRemove<T>(Action<object> fun) where T : IComponent => UnsubscribeOnRemove(typeof(T), fun);
        public void SubscribeOnRemove(Type type, Action<object> fun) => onRemoveActions[type] += fun;
        public void UnsubscribeOnRemove(Type type, Action<object> fun) => onRemoveActions[type] -= fun;
    }
}