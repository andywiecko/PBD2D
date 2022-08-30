using andywiecko.BurstMathUtils;
using andywiecko.ECS;
using UnityEngine;

namespace andywiecko.PBD2D.Core
{
    public class MouseInteractionConfigurationHolder : ConfigurationHolder<MouseInteractionConfiguration>
    {
        private void OnDrawGizmos()
        {
            if (!Application.isPlaying || !Configuration.IsPressed)
            {
                return;
            }

            Gizmos.color = Color.blue;
            var p = Configuration.MousePosition;
            var r = Configuration.Radius;
            var n = Configuration.MouseRotation.Value;
            var t = MathUtils.Rotate90CCW(n);
            GizmosUtils.DrawCircle(p, r);
            Gizmos.color = Color.red;
            GizmosUtils.DrawRay(p, +n * r);
            GizmosUtils.DrawRay(p, -n * r);
            GizmosUtils.DrawRay(p, +t * r);
            GizmosUtils.DrawRay(p, -t * r);
        }
    }
}