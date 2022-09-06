using andywiecko.BurstMathUtils;
using andywiecko.ECS;
using andywiecko.PBD2D.Core;
using Unity.Mathematics;
using UnityEngine;

namespace andywiecko.PBD2D.Components
{
    public class Ground : Entity
    {
        public Line Line => new
        (
            point: transform.position.ToFloat2(),
            normal: transform.up.ToFloat2()
        );

        public float2 Displacement => position - previousPosition;

        public IPhysicalMaterial PhysicalMaterial => physicalMaterial ? physicalMaterial : Core.PhysicalMaterial.Default;
        [SerializeField]
        private PhysicalMaterial physicalMaterial = default;

        [SerializeField]
        private Transform overrideDisplacementTransform = default;

        private float2 position;
        private float2 previousPosition;

        private void Update()
        {
            previousPosition = position;
            var t = overrideDisplacementTransform == null ? transform : overrideDisplacementTransform;
            position = t.position.ToFloat2();
        }

        private void OnDrawGizmos()
        {
            var (p, n) = Line;
            var t = n.Rotate90CW();
            var l = 100;

            Gizmos.color = Color.black;
            GizmosUtils.DrawRay(p, +l * t);
            GizmosUtils.DrawRay(p, -l * t);

            Gizmos.color = Color.green;
            GizmosUtils.DrawRay(p, 0.5f * n);
        }
    }
}