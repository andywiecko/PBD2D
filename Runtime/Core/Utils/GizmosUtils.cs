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

        public static void DrawRectangle(float2 min, float2 max, float z = 0)
        {
            var (minx, miny) = min;
            var (maxx, maxy) = max;

            var pA = math.float2(min);
            var pB = math.float2(maxx, miny);
            var pC = math.float2(max);
            var pD = math.float2(minx, maxy);

            DrawLine(pA, pB, z);
            DrawLine(pB, pC, z);
            DrawLine(pC, pD, z);
            DrawLine(pD, pA, z);
        }

        public static void DrawCapsule(float2 pA, float2 pB, float radius, float z = 0)
        {
            DrawCircle(pA, radius, z);
            DrawCircle(pB, radius, z);
            var pAB = pB - pA;
            var pR = radius * math.normalize(MathUtils.Rotate90CW(pAB));
            DrawLine(pA + pR, pB + pR, z);
            DrawLine(pA - pR, pB - pR, z);
        }

        public static void DrawArrow(float2 pA, float2 pB, float headSizeRatio = 0.33f)
        {
            Gizmos.DrawLine(pA.ToFloat3(), pB.ToFloat3());

            var len = math.distance(pA, pB);
            // TODO
        }
    }
}