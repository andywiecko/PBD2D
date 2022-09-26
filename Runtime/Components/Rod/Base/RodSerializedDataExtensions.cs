using andywiecko.BurstCollections;
using andywiecko.PBD2D.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;

namespace andywiecko.PBD2D.Components
{
    public static class RodSerializedDataExtensions
    {
        public static Point[] ToPoints(this RodSerializedData data) => Enumerable
            .Range(0, data.Positions.Length).Select(i => new Point((Id<Point>)i)).ToArray();

        public static float2[] ToPositions(this RodSerializedData data, Func<float2, float2> transformation) =>
            data.Positions.Select(i => transformation(i)).ToArray();

        public static float[] ToWeights(this RodSerializedData data, Func<float2, float2> transformation, float density)
        {
            var pointsCount = data.Positions.Length;
            var weights = new float[pointsCount];
            var positions = data.ToPositions(transformation);
            foreach (var e in data.GetEdges())
            {
                var (a, b) = e;
                var length = e.GetLength(positions);
                var w0 = 2f / density / length; // 2 (points)
                weights[(int)a] += w0;
                weights[(int)b] += w0;
            }

            return weights;
        }

        public static Edge[] ToEdges(this RodSerializedData data) => data.GetEdges().ToArray();

        private static IEnumerable<Edge> GetEdges(this RodSerializedData data) => Enumerable
            .Range(0, data.Edges.Length / 2)
            .Select(i => (Edge)(data.Edges[2 * i], data.Edges[2 * i + 1]));
    }
}