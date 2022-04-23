using andywiecko.BurstCollections;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace andywiecko.PBD2D.Core
{
    public abstract class BaseComponent : MonoBehaviour, IComponent
    {
        public Entity Entity { get; private set; } = default;
        public World World => Entity.World;

        public Id<IComponent> Id { get; } = ComponentIdCounter.GetNext();

        private readonly List<IDisposable> refsToDisposeOnDestroy = new();

        protected void DisposeOnDestroy(params IDisposable[] references)
        {
            foreach (var reference in references)
            {
                refsToDisposeOnDestroy.Add(reference);
            }
        }

        protected virtual void Awake() => Entity = GetComponent<Entity>();
        protected virtual void OnEnable() => World.ComponentsRegistry.Add(this);
        protected virtual void OnDisable() => World.ComponentsRegistry.Remove(this);
        protected virtual void OnDestroy()
        {
            foreach (var reference in refsToDisposeOnDestroy)
            {
                reference.Dispose();
            }
        }
    }
}
