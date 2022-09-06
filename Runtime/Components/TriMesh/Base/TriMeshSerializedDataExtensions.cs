using andywiecko.BurstCollections;
using andywiecko.PBD2D.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;

namespace andywiecko.PBD2D.Components
{
    public static class TriMeshSerializedDataExtensions
    {
        public static Point[] ToPoints(this TriMeshSerializedData data) => Enumerable
            .Range(0, data.Positions.Length).Select(i => new Point((Id<Point>)i)).ToArray();

        public static float2[] ToPositions(this TriMeshSerializedData data, Func<float2, float2> transformation) =>
            data.Positions.Select(i => transformation(i)).ToArray();

        public static float[] ToWeights(this TriMeshSerializedData data, Func<float2, float2> transformation, float density)
        {
            var pointsCount = data.Positions.Length;
            var weights = new float[pointsCount];
            var triangles = data.ToTriangles();
            var positions = data.ToPositions(transformation);
            foreach (var t in triangles)
            {
                var (a, b, c) = t;
                var area = t.GetSignedArea2(positions);
                var w0 = 6f / density / math.abs(area); // 3 (points) * 2 (doubled area)
                weights[(int)a] += w0;
                weights[(int)b] += w0;
                weights[(int)c] += w0;
            }

            return weights;
        }

        public static Triangle[] ToTriangles(this TriMeshSerializedData data) => data
            .GetTriangles()
            .Select(i => (Triangle)(i.x, i.y, i.z))
            .ToArray();

        public static Edge[] ToEdges(this TriMeshSerializedData data) => data
            .GetTriangles()
            .SelectMany(i => i.UnpackSorted())
            .Distinct()
            .ToArray();

        public static ExternalEdge[] ToExternalEdges(this TriMeshSerializedData data) => data
            .GetTriangles()
            .SelectMany(i => i.UnpackUnsorted())
            .GroupBy(i => i, new EdgeComparer())
            .Where(g => g.Count() == 1)
            .Select(g => g.Key)
            .Select(i => new ExternalEdge(i.IdA, i.IdB))
            .ToArray();

        private static IEnumerable<int3> GetTriangles(this TriMeshSerializedData data) => Enumerable
            .Range(0, data.Triangles.Length / 3)
            .Select(i => math.int3(data.Triangles[3 * i], data.Triangles[3 * i + 1], data.Triangles[3 * i + 2]));

        private static IEnumerable<Edge> UnpackSorted(this int3 t)
        {
            var min = math.cmin(t);
            var max = math.cmax(t);
            var med = math.csum(t) - min - max;
            yield return (min, max);
            yield return (min, med);
            yield return (med, max);
        }

        private static IEnumerable<Edge> UnpackUnsorted(this int3 t)
        {
            yield return (t.x, t.y);
            yield return (t.y, t.z);
            yield return (t.z, t.x);
        }

        private readonly struct EdgeComparer : IEqualityComparer<Edge>
        {
            public bool Equals(Edge x, Edge y) => x.Contains(y.IdA) && x.Contains(y.IdB);
            public int GetHashCode(Edge obj) => obj.GetHashCode();
        }
    }
}