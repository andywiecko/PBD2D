using System.Collections.Generic;
using Unity.Jobs;
using UnityEngine;

namespace andywiecko.PBD2D.Core
{
    public abstract class BaseSystem<TComponent> : MonoBehaviour, ISystem
        where TComponent : IComponent
    {
        protected IReadOnlyList<TComponent> References => ComponentsRegistry<TComponent>.Components();
        public abstract JobHandle Schedule(JobHandle dependencies);
        protected void OnEnable() => SystemsRegistry.Add(this);
        protected void OnDisable() => SystemsRegistry.Remove(this);
    }
}