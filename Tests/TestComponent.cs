using andywiecko.BurstCollections;
using andywiecko.ECS;
using System;

namespace andywiecko.PBD2D.Editor.Tests
{
    public abstract class TestComponent : IComponent, IDisposable
    {
        private static readonly IdCounter<IComponent> counter = new();
        public Id<IComponent> ComponentId { get; } = counter.GetNext();
        public virtual void Dispose() { }
    }
}