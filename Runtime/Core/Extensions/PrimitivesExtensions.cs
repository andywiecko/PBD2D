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
            _ => throw new System.ArgumentOutOfRangeException()
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

        public static bool PointInside(this Triangle triangle, float2 point, NativeIndexedArray<Id<Point>, float2>.ReadOnly positions)
        {
            var (idA, idB, idC) = triangle;
            var (a, b, c) = (positions[idA], positions[idB], positions[idC]);
            return MathUtils.PointInsideTriangle(point, a, b, c);
        }

        public static float2 GetCenter(this Triangle triangle, NativeIndexedArray<Id<Point>, float2>.ReadOnly positions)
        {
            var (idA, idB, idC) = triangle;
            return (positions[idA] + positions[idB] + positions[idC]) / 3;
        }

        public static Triangle[] ToTrianglesArray(this int[] tris) => Enumerable
            .Range(0, tris.Length / 3)
            .Select(i => (Triangle)(tris[3 * i], tris[3 * i + 1], tris[3 * i + 2]))
            .ToArray();

        public static Edge[] ToEdgesArray(this int[] edges) => Enumerable
            .Range(0, edges.Length / 2)
            .Select(i => (Edge)(edges[2 * i], edges[2 * i + 1]))
            .ToArray();
    }
}