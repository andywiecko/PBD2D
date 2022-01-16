using andywiecko.BurstCollections;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using UnityEngine;

namespace andywiecko.PBD2D.Core
{
    public abstract class ComponentsTuple
    {
        protected readonly static Dictionary<(Type itemType1, Type itemType2), List<Type>> genericArgumentsToTypes = new();
        protected readonly static Dictionary<Type, MethodInfo> typeToSetUpMethodInfo = new();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void Initialize()
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (typeof(ComponentsTuple).IsAssignableFrom(type) && type.IsAbstract == false)
                    {
                        Activator.CreateInstance(type); // Runs static constructor for a type.
                        GenerateReflectionMappings(type);
                    }
                }
            }
        }

        private static void GenerateReflectionMappings(Type type)
        {
            var baseType = type.BaseType;
            var genericArguments = baseType.GetGenericArguments();
            var method = baseType.GetMethod("SetUp", BindingFlags.Instance | BindingFlags.NonPublic);
            typeToSetUpMethodInfo.Add(type, method);

            switch (genericArguments.Length)
            {
                case 2:

                    var tuple = (genericArguments[0], genericArguments[1]);
                    if (genericArgumentsToTypes.ContainsKey(tuple))
                    {
                        genericArgumentsToTypes[tuple].Add(type);
                    }
                    else
                    {
                        genericArgumentsToTypes.Add(tuple, new List<Type>() { type });
                    }

                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        public Id<IComponent> Id { get; } = ComponentIdCounter.GetNext();
        private readonly List<IDisposable> refsToDisposeOnDestroy = new();

        protected void DisposeOnDestroy(params IDisposable[] references)
        {
            foreach (var reference in references)
            {
                refsToDisposeOnDestroy.Add(reference);
            }
        }

        protected void Destroy()
        {
            foreach (var reference in refsToDisposeOnDestroy)
            {
                reference.Dispose();
            }
        }
    }

    public abstract class ComponentsTuple<T1, T2> : ComponentsTuple
        where T1 : IComponent
        where T2 : IComponent
    {
        protected T1 Item1 { get; private set; }
        protected T2 Item2 { get; private set; }

        static ComponentsTuple()
        {
            ComponentsRegistry<T1>.OnAddComponent += OnAddComponentItem1;
            ComponentsRegistry<T2>.OnAddComponent += OnAddComponentItem2;
        }

        private static void CreateTuple(T1 item1, T2 item2)
        {
            var tuple = (typeof(T1), typeof(T2));
            var types = genericArgumentsToTypes[tuple];
            foreach (var type in types)
            {
                var instance = Activator.CreateInstance(type);
                var method = typeToSetUpMethodInfo[type];
                method.Invoke(instance, new object[] { item1, item2 });
            }
        }

        public void Deconstruct(out T1 item1, out T2 item2)
        {
            item1 = Item1;
            item2 = Item2;
        }

        protected virtual void Initialize() { }
        protected virtual bool InstantiateWhen(T1 item1, T2 item2) => true;

        [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Calls implicit by reflection.")]
        private void SetUp(T1 item1, T2 item2)
        {
            if (!InstantiateWhen(item1, item2))
            {
                return;
            }

            Item1 = item1;
            Item2 = item2;

            Initialize();
            ComponentsRegistry.Register(this);
            ComponentsRegistry<T1>.OnRemoveComponent += OnRemoveComponentItem1;
            ComponentsRegistry<T2>.OnRemoveComponent += OnRemoveComponentItem2;
        }

        private static void OnAddComponentItem1(T1 item1)
        {
            foreach (var item2 in ComponentsRegistry<T2>.Components())
            {
                CreateTuple(item1, item2);
            }
        }

        private static void OnAddComponentItem2(T2 item2)
        {
            foreach (var item1 in ComponentsRegistry<T1>.Components())
            {
                CreateTuple(item1, item2);
            }
        }

        private void OnRemoveComponentItem1(T1 item1)
        {
            if (Item1.Equals(item1))
            {
                RemoveTuple();
            }
        }

        private void OnRemoveComponentItem2(T2 item2)
        {
            if (Item2.Equals(item2))
            {
                RemoveTuple();
            }
        }

        private void RemoveTuple()
        {
            ComponentsRegistry.Deregister(this);
            ComponentsRegistry<T1>.OnRemoveComponent -= OnRemoveComponentItem1;
            ComponentsRegistry<T2>.OnRemoveComponent -= OnRemoveComponentItem2;
            Destroy();
        }
    }
}
