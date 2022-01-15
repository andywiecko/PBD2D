using andywiecko.BurstMathUtils;
using andywiecko.PBD2D.Core;
using UnityEngine;

namespace andywiecko.PBD2D.Components
{
    public class Ground : BaseComponent
    {
        public Surface Surface => new Surface
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
            Gizmos.DrawRay(point.ToFloat3(), +(length * tangent).ToFloat3());
            Gizmos.DrawRay(point.ToFloat3(), -(length * tangent).ToFloat3());
        }
    }
}