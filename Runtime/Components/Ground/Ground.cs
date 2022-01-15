using andywiecko.BurstMathUtils;
using andywiecko.PBD2D.Core;
using UnityEngine;

namespace andywiecko.PBD2D.Components
{
    public class Ground : BaseComponent
    {
        public Line Surface => new 
        (
            point: transform.position.ToFloat2(),
            normal: transform.up.ToFloat2()
        );

        private void OnDrawGizmos()
        {
            var (point, normal) = Surface;
            var tangent = normal.Rotate90CW();
            var length = 100;
            Gizmos.color = Color.black;
            var p = point.ToFloat3();
            var n = normal.ToFloat3();
            Gizmos.DrawRay(p, +(length * tangent).ToFloat3());
            Gizmos.DrawRay(p, -(length * tangent).ToFloat3());
            Gizmos.color = Color.green;
            Gizmos.DrawRay(p, 0.5f * n);
        }
    }
}