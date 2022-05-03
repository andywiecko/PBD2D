using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace andywiecko.PBD2D.Core
{
    public class ComponentsRegistry
    {
        #region Statics
        private static readonly Dictionary<Type, IReadOnlyList<Type>> derivedTypesToInterfaces = new();
        private static readonly HashSet<Type> staticComponentInterfaces = new();
        private static readonly List<(Type t, Type t1, Type t2)> tupleTypes = new();

        static ComponentsRegistry()
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in assembly.GetTypes())
                {
                    RegisterTypeInterfaces(type);
                }
            }

            RegisterTupleTypes();
        }

        private static void RegisterTypeInterfaces(Type type)
        {
            var list = new List<Type>();
            foreach (var @interface in type.GetInterfaces())
            {
                if (typeof(IComponent).IsAssignableFrom(@interface) &&
                    @interface.ContainsGenericParameters == false)
                {
                    list.Add(@interface);
                    staticComponentInterfaces.Add(@interface);
                }
            }
            derivedTypesToInterfaces.TryAdd(type, list);
        }

        private static void RegisterTupleTypes()
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (typeof(ComponentsTuple).IsAssignableFrom(type) && type.IsAbstract == false)
                    {
                        var baseType = type.BaseType;
                        var args = baseType.GetGenericArguments();
                        var tuple = (type, args[0], args[1]);
                        tupleTypes.Add(tuple);
                    }
                }
            }
        }
        #endregion

        private readonly Dictionary<Type, IList> components = new();
        private readonly Dictionary<Type, Action<object>> onAddActions = new();
        private readonly Dictionary<Type, Action<object>> onRemoveActions = new();

        public ComponentsRegistry()
        {
            components = staticComponentInterfaces.ToDictionary(i => i, i => CreateListOf(i));
            onAddActions = staticComponentInterfaces.ToDictionary(i => i, i => default(Action<object>));
            onRemoveActions = staticComponentInterfaces.ToDictionary(i => i, i => default(Action<object>));

            foreach (var (t, t1, t2) in tupleTypes)
            {
                SubscribeOnAdd(t1, (object i) => OnAddComponentItem1(i, t, t2));
                SubscribeOnAdd(t2, (object i) => OnAddComponentItem2(i, t, t1));
            }

            static IList CreateListOf(Type t) => Activator.CreateInstance(typeof(List<>).MakeGenericType(t)) as IList;
        }

        public IReadOnlyList<T> GetComponents<T>() where T : IComponent => components[typeof(T)] as List<T>;
        public IEnumerable GetComponents(Type type) => components[type];

        public void Add<T>(T instance) where T : IComponent
        {
            var type = instance.GetType();
            foreach (var @interface in derivedTypesToInterfaces[type])
            {
                components[@interface].Add(instance);
                onAddActions[@interface]?.Invoke(instance);
            }
        }

        public void Remove<T>(T instance) where T : IComponent
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

        private void OnAddComponentItem1(object item1, Type t, Type t2)
        {
            foreach (var item2 in GetComponents(t2))
            {
                Activator.CreateInstance(t, item1, item2, this);
            }
        }

        private void OnAddComponentItem2(object item2, Type t, Type t1)
        {
            foreach (var item1 in GetComponents(t1))
            {
                Activator.CreateInstance(t, item1, item2, this);
            }
        }
    }
}