using UnityEngine;

namespace andywiecko.PBD2D.Core
{
    public interface IPhysicalMaterial
    {
        float Friction { get; }
        float Density { get; }
    }

    [CreateAssetMenu(fileName = "PhysicalMaterial", menuName = "PBD2D/Physical Material")]
    public class PhysicalMaterial : ScriptableObject, IPhysicalMaterial
    {
        private class InternalDefault : IPhysicalMaterial
        {
            public float Friction => 1;
            public float Density => 1;
        }

        public static readonly IPhysicalMaterial Default = new InternalDefault();

        [field: SerializeField, Min(0)]
        public float Friction { get; private set; } = Default.Friction;

        [field: SerializeField, Min(0)]
        public float Density { get; private set; } = Default.Density;
    }
}