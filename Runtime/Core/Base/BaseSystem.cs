using System.Collections.Generic;
using Unity.Jobs;
using UnityEngine;

namespace andywiecko.PBD2D.Core
{
    public abstract class BaseSystem<TComponent> : MonoBehaviour, ISystem
        where TComponent : IComponent
    {
        [field: SerializeField]
        public World World { get; private set; }
        protected SimulationConfiguration Configuration => World.Configuration;
        protected IEnumerable<TComponent> References => World.ComponentsRegistry.GetComponents<TComponent>();
        public abstract JobHandle Schedule(JobHandle dependencies);
        protected void OnEnable() => World.SystemsRegistry.Add(this);
        protected void OnDisable() => World.SystemsRegistry.Remove(this);

        private void OnValidate()
        {
            var manager = GetComponentInParent<SystemsManager>();
            World = manager.World;
        }
    }
}