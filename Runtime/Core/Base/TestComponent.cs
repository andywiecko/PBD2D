using andywiecko.BurstCollections;
using System;

namespace andywiecko.PBD2D.Core
{
    // move to test assembly?
    public abstract class TestComponent : IComponent, IDisposable
    {
        public Id<IComponent> Id { get; } = ComponentIdCounter.GetNext();
        public virtual void Dispose() { }
    }
}