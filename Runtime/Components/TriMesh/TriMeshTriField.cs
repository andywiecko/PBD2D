using andywiecko.BurstMathUtils;
using andywiecko.PBD2D.Core;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace andywiecko.PBD2D.Components
{
    public interface ITriField
    {

    }

    public class TriMeshTriField : MonoBehaviour
    {
        public int samples = 10;

        public float2 p = new(-0.08f, 0.19f);

        private List<float3> bars;

        public float2 a = math.float2(0, 0);
        public float2 b = math.float2(1, 2);
        public float2 c = math.float2(-3, 1);

        public void Awake()
        {
            bars = new List<float3>();

            // generate bars
            var a = math.float2(0, 0);
            var b = math.float2(0, 1);
            var c = math.float2(1, 1);
            var dx = 1f / (samples - 1);
            for (int j = 0; j < samples; j++)
            {
                for (int i = 0; i <= j; i++)
                {
                    var p = math.clamp(math.float2(dx * i, dx * j), 0, 1);
                    var bar = MathUtils.Barycentric(a, b, c, p);
                    bars.Add(bar);
                }
            }
        }

        public void OnDrawGizmos()
        {
            if (!Application.isPlaying) return;

            Gizmos.color = Color.red;

            Gizmos.DrawLine(a.ToFloat3(), b.ToFloat3());
            Gizmos.DrawLine(b.ToFloat3(), c.ToFloat3());
            Gizmos.DrawLine(c.ToFloat3(), a.ToFloat3());

            Gizmos.color = Color.blue;
            foreach (var bar in bars)
            {
                var p = a * bar.x + b * bar.y + c * bar.z;
                Gizmos.DrawSphere(p.ToFloat3(), 0.03f);
            }

            Gizmos.color = Color.red;
            Gizmos.DrawSphere(p.ToFloat3(), 0.03f);

            // field lookup
            var bbb = MathUtils.Barycentric(a, b, c, p);

            bbb = math.clamp(bbb, 0, 1);
            if (math.all(bbb == 0)) bbb.x = 1;
            bbb /= math.csum(bbb);

            var pp = bbb.x * math.float2(0, 0) + bbb.y * math.float2(0, 1) + bbb.z * math.float2(1, 1);
            Debug.Log(pp);
            var dx = 1f / (samples - 1);
            pp /= dx;
            pp = math.round(pp);

            var id = (int2)pp.xy;
            var index = MathUtils.ConvertToTriMatId(id.x, id.y);
            var bbar = bars[index];
            var q = bbar.x * a + bbar.y * b + bbar.z * c;
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(q.ToFloat3(), 0.03f);
        }
    }
}