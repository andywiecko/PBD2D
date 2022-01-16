using andywiecko.BurstCollections;
using System;

namespace andywiecko.PBD2D.Core
{
    public abstract class FreeComponent : IComponent, IDisposable
    {
        public Id<IComponent> Id { get; } = ComponentIdCounter.GetNext();
        public FreeComponent() => ComponentsRegistry.Register(this);
        public virtual void Dispose() => ComponentsRegistry.Deregister(this);
    }
}