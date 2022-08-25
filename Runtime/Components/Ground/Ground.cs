using andywiecko.BurstMathUtils;
using andywiecko.ECS;
using andywiecko.PBD2D.Core;
using UnityEngine;

namespace andywiecko.PBD2D.Components
{
    public class Ground : Entity
    {
        public IPhysicalMaterial PhysicalMaterial => physicalMaterial ? physicalMaterial : Core.PhysicalMaterial.Default;
        [SerializeField]
        private PhysicalMaterial physicalMaterial = default;

        public Line Surface => new
        (
            point: transform.position.ToFloat2(),
            normal: transform.up.ToFloat2()
        );

        private void OnDrawGizmos()
        {
            var (p, n) = Surface;
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