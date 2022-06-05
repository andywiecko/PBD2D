using System.Collections.Generic;
using Unity.Jobs;

namespace andywiecko.PBD2D.Core
{
    public abstract class BaseSystem<TComponent> : ISystem
        where TComponent : IComponent
    {
        public IWorld World { get; set; }
        protected SimulationConfiguration Configuration => World.ConfigurationsRegistry.Get<SimulationConfiguration>();
        protected IReadOnlyList<TComponent> References => World.ComponentsRegistry.GetComponents<TComponent>();
        public abstract JobHandle Schedule(JobHandle dependencies);
        public void Run() => Schedule(default).Complete();
    }
}