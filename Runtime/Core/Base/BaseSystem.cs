using System.Collections.Generic;
using Unity.Jobs;
using UnityEngine;

namespace andywiecko.PBD2D.Core
{
    public abstract class BaseSystem<TComponent> : MonoBehaviour, ISystem
        where TComponent : IComponent
    {
        public IWorld World { get; set; }
        protected ISimulationConfiguration Configuration => World.Configuration;
        protected IReadOnlyList<TComponent> References => World.ComponentsRegistry.GetComponents<TComponent>();
        public abstract JobHandle Schedule(JobHandle dependencies);
        protected void Awake() => World = GetComponentInParent<SystemsManager>().World;
        protected void OnEnable() => World.SystemsRegistry.Add(this);
        protected void OnDisable() => World.SystemsRegistry.Remove(this);
    }
}