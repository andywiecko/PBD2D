using andywiecko.BurstCollections;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace andywiecko.PBD2D.Core
{
    public abstract class BaseComponent : MonoBehaviour
    {
        private static int id = 0;
        public Id<IComponent> Id { get; } = (Id<IComponent>)id++;

        private readonly List<IDisposable> refsToDisposeOnDestroy = new();

        protected void DisposeOnDestroy(params IDisposable[] references)
        {
            foreach (var reference in references)
            {
                refsToDisposeOnDestroy.Add(reference);
            }
        }

        protected virtual void OnDestroy()
        {
            foreach (var reference in refsToDisposeOnDestroy)
            {
                reference.Dispose();
            }
        }

        protected virtual void OnEnable() => ComponentsRegistry.Register(this);
        protected virtual void OnDisable() => ComponentsRegistry.Deregister(this);
    }
}
