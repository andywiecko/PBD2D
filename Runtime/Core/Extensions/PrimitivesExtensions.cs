using andywiecko.BurstCollections;
using andywiecko.BurstMathUtils;
using System.Linq;
using Unity.Mathematics;

namespace andywiecko.PBD2D.Core
{
    public static class PrimitivesExtensions
    {
        public static float GetSignedArea2(this Triangle triangle, float2[] positions)
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

        public static float2 GetCenter(this Edge edge, NativeIndexedArray<Id<Point>, float2>.ReadOnly positions)
        {
            var (idA, idB) = edge;
            return 0.5f * (positions[idA] + positions[idB]);
        }

        public static Edge ToEdge(this ExternalEdge edge) => (edge.IdA, edge.IdB);

        public static AABB ToAABB(this Triangle triangle, NativeIndexedArray<Id<Point>, float2>.ReadOnly positions, float margin = 0f)
        {
            var (idA, idB, idC) = triangle;
            var (pA, pB, pC) = (positions[idA], positions[idB], positions[idC]);
            return new AABB
            (
                min: MathUtils.Min(pA, pB, pC) - margin,
                max: MathUtils.Max(pA, pB, pC) + margin
            );
        }

        public static AABB ToAABB(this Edge edge, NativeIndexedArray<Id<Point>, float2>.ReadOnly positions, float margin = 0f)
        {
            var (pA, pB) = positions.At(edge);
            return new AABB
            (
                min: math.min(pA, pB) - margin,
                max: math.max(pA, pB) + margin
            );
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