using andywiecko.BurstMathUtils;
using Unity.Mathematics;
using UnityEngine;

namespace andywiecko.PBD2D.Core
{
    public static class GizmosUtils
    {
        public static void DrawLine(float2 a, float2 b, float z = 0)
        {

        }

        public static void DrawRectangle(float2 min, float2 max)
        {
            var (minx, miny) = min;
            var (maxx, maxy) = max;

            var pA = math.float3(min, 0);
            var pB = math.float3(maxx, miny, 0);
            var pC = math.float3(max, 0);
            var pD = math.float3(minx, maxy, 0);

            Gizmos.DrawLine(pA, pB);
            Gizmos.DrawLine(pB, pC);
            Gizmos.DrawLine(pC, pD);
            Gizmos.DrawLine(pD, pA);
        }

        public static void DrawCapsule(float2 pA, float2 pB, float radius)
        {
            Gizmos.DrawWireSphere(pA.ToFloat3(), radius);
            Gizmos.DrawWireSphere(pB.ToFloat3(), radius);
            var pAB = pB - pA;
            var pR = radius * math.normalize(MathUtils.Rotate90CW(pAB));
            Gizmos.DrawLine((pA + pR).ToFloat3(), (pB + pR).ToFloat3());
            Gizmos.DrawLine((pA - pR).ToFloat3(), (pB - pR).ToFloat3());
        }

        public static void DrawArrow(float2 pA, float2 pB, float headSizeRatio = 0.33f)
        {
            Gizmos.DrawLine(pA.ToFloat3(), pB.ToFloat3());

            var len = math.distance(pA, pB);
            // TODO
        }
    }
}