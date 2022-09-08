using andywiecko.BurstMathUtils;
using Unity.Mathematics;
using UnityEngine;

namespace andywiecko.PBD2D.Core
{
    public static class GizmosUtils
    {
        public static void DrawLine(float2 a, float2 b, float z = 0) => Gizmos.DrawLine(a.ToFloat3(z), b.ToFloat3(z));
        public static void DrawRay(float2 a, float2 n, float z = 0) => Gizmos.DrawRay(a.ToFloat3(z), n.ToFloat3());
        public static void DrawCircle(float2 a, float r, float z = 0) => Gizmos.DrawWireSphere(a.ToFloat3(z), r);

        public static void DrawTriangle(float2 a, float2 b, float2 c, float z = 0)
        {
            DrawLine(a, b, z);
            DrawLine(b, c, z);
            DrawLine(c, a, z);
        }

        public static void DrawRectangle(float2 min, float2 max, float z = 0)
        {
            var (minx, miny) = min;
            var (maxx, maxy) = max;

            var a = math.float2(min);
            var b = math.float2(maxx, miny);
            var c = math.float2(max);
            var d = math.float2(minx, maxy);

            DrawQuadrilateral(a, b, c, d, z);
        }

        public static void DrawQuadrilateral(float2 a, float2 b, float2 c, float2 d, float z = 0)
        {
            DrawLine(a, b, z);
            DrawLine(b, c, z);
            DrawLine(c, d, z);
            DrawLine(d, a, z);
        }

        public static void DrawCapsule(float2 a, float2 b, float r, float z = 0)
        {
            DrawCircle(a, r, z);
            DrawCircle(b, r, z);
            var pAB = b - a;
            var pR = r * math.normalize(MathUtils.Rotate90CW(pAB));
            DrawLine(a + pR, b + pR, z);
            DrawLine(a - pR, b - pR, z);
        }
    }
}