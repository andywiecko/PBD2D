using andywiecko.BurstCollections;
using andywiecko.BurstMathUtils;
using System;
using System.Linq;
using Unity.Mathematics;

namespace andywiecko.PBD2D.Core
{
    public static class PrimitivesExtensions
    {
        public static float2 GetPoint(this AABB aabb, int i) => i switch
        {
            0 => aabb.Min,
            1 => aabb.Max,
            2 => new(aabb.Min.x, aabb.Max.y),
            3 => new(aabb.Max.x, aabb.Min.y),
            _ => throw new ArgumentOutOfRangeException()
        };

        public static bool IsAboveLine(this AABB aabb, Line line)
        {
            var (a, n) = line;
            for (int k = 0; k < 4; k++)
            {
                var p = aabb.GetPoint(k);
                if (MathUtils.PointLineSignedDistance(p, n, a) <= 0)
                {
                    return false;
                }
            }

            return true;
        }

        public static float GetSignedArea2(this Triangle triangle, ReadOnlySpan<float2> positions)
        {
            var (pIdA, pIdB, pIdC) = triangle;
            var (pA, pB, pC) = (positions[(int)pIdA], positions[(int)pIdB], positions[(int)pIdC]);
            return MathUtils.TriangleSignedArea2(pA, pB, pC);
        }
    }
}