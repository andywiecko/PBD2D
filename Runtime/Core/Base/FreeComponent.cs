using andywiecko.BurstCollections;
using System;

namespace andywiecko.PBD2D.Core
{
    public abstract class FreeComponent : IComponent, IDisposable
    {
        private World world;
        public Id<IComponent> Id { get; } = ComponentIdCounter.GetNext();
        public FreeComponent() => world.ComponentsRegistry.Register(this);
        public virtual void Dispose() => world.ComponentsRegistry.Deregister(this);
    }
}