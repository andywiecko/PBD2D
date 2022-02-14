using UnityEngine;

namespace andywiecko.PBD2D.Core
{
    public interface IPhysicalMaterial
    {
        float Friction { get; }
        float Restitution { get; }
    }

    [PreferBinarySerialization]
    [CreateAssetMenu(fileName = "PhysicalMaterial", menuName = "PBD2D/Physical Material")]
    public class PhysicalMaterial : ScriptableObject, IPhysicalMaterial
    {
        public class Default : IPhysicalMaterial
        {
            public static readonly Default Instance = new();
            public float Friction => InternalDefault.Friction;
            public float Restitution => InternalDefault.Restitution;
        }

        private static class InternalDefault
        {
            public const float Friction = 0.5f;
            public const float Restitution = 0.5f;
        }

        [field: SerializeField, Min(0)]
        public float Friction { get; private set; } = InternalDefault.Friction;

        [field: SerializeField, Range(0, 1)]
        public float Restitution { get; private set; } = InternalDefault.Restitution;
    }
}