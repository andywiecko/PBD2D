using andywiecko.BurstCollections;
using andywiecko.PBD2D.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Mathematics;

namespace andywiecko.PBD2D.Components
{
    public static class EdgeMeshSerializedDataExtensions
    {
        public static Point[] ToPoints(this EdgeMeshSerializedData data) => Enumerable
            .Range(0, data.Positions.Length).Select(i => new Point((Id<Point>)i)).ToArray();

        public static float2[] ToPositions(this EdgeMeshSerializedData data, Func<float2, float2> transformation) =>
            data.Positions.Select(i => transformation(i)).ToArray();

        public static float[] ToWeights(this EdgeMeshSerializedData data, Func<float2, float2> transformation, float density)
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

        public static Edge[] ToEdges(this EdgeMeshSerializedData data) => data.GetEdges().ToArray();

        private static IEnumerable<Edge> GetEdges(this EdgeMeshSerializedData data) => Enumerable
            .Range(0, data.Edges.Length / 2)
            .Select(i => (Edge)(data.Edges[2 * i], data.Edges[2 * i + 1]));

        public static NativeStackedLists<Id<Point>> ToSegments(this EdgeMeshSerializedData data, Allocator allocator)
        {
            var segments = new NativeStackedLists<Id<Point>>(allocator);

            using var edges = new NativeIndexedArray<Id<Edge>, Edge>(data.ToEdges(), Allocator.Temp);
            var pointsLength = data.ToPoints().Length;

            var neighbors = new NativeMultiHashMap<Id<Point>, Id<Point>>(capacity: pointsLength * pointsLength, Allocator.Temp);
            var edgeToId = new NativeHashMap<Edge, Id<Edge>>(2 * edges.Length, Allocator.Temp);
            var visited = new NativeIndexedArray<Id<Edge>, bool>(edges.Length, Allocator.Temp);
            var degrees = new NativeIndexedArray<Id<Point>, int>(pointsLength, Allocator.Temp);

            foreach (var (id, (a, b)) in edges.IdsValues)
            {
                neighbors.Add(a, b);
                neighbors.Add(b, a);
                edgeToId.Add(new(a, b), id);
                edgeToId.Add(new(b, a), id);
            }

            var pointsToIterate = new List<(Point, int)>();
            foreach (var i in 0..pointsLength)
            {
                var id = (Id<Point>)i;
                var n = neighbors.GetValuesForKey(id);
                var count = 0;
                foreach (var _ in n)
                {
                    count++;
                }

                degrees[id] = count;

                pointsToIterate.Add((new(i), count));
            }

            var queue = new Queue<(Point, int)>(pointsToIterate
                .OrderByDescending(i => i.Item2 == 2 ? -1 : i.Item2)
            );

            while (queue.TryDequeue(out var p))
            {
                var id0 = p.Item1.Id;
                foreach (var id1 in neighbors.GetValuesForKey(id0))
                {
                    var edge = new Edge(id0, id1);
                    var eId = edgeToId[edge];
                    if (!visited[eId])
                    {
                        BuildSegment(id0, id1);
                    }
                }
            }

            void BuildSegment(Id<Point> id0, Id<Point> id1)
            {
                Id<Edge> GetEdgeId(Id<Point> id0, Id<Point> id1) => edgeToId[new(id0, id1)];

                bool TryGetNextPoint(Id<Point> id0, out Id<Point> id1)
                {
                    foreach (var id in neighbors.GetValuesForKey(id0))
                    {
                        var eId = GetEdgeId(id0, id);
                        if (!visited[eId])
                        {
                            id1 = id;
                            return true;
                        }
                    }
                    id1 = Id<Point>.Invalid;
                    return false;
                }

                segments.Push();

                segments.Add(id0);
                segments.Add(id1);
                var eId = GetEdgeId(id0, id1);
                visited[eId] = true;

                if (degrees[id1] != 2)
                {
                    return;
                }

                do
                {
                    id0 = id1;
                    if (!TryGetNextPoint(id0, out id1))
                    {
                        return;
                    }

                    segments.Add(id1);
                    eId = GetEdgeId(id0, id1);
                    visited[eId] = true;

                } while (degrees[id1] == 2);
            }

            edgeToId.Dispose();
            visited.Dispose();
            neighbors.Dispose();
            degrees.Dispose();

            return segments;
        }

        public static NativeList<Stencil> ToStencils(this EdgeMeshSerializedData data, Allocator allocator)
        {
            var stencils = new NativeList<Stencil>(64, allocator);

            var pointsLength = data.ToPoints().Length;
            using var points = new NativeArray<Point>(data.ToPoints(), Allocator.Temp);
            using var edges = new NativeIndexedArray<Id<Edge>, Edge>(data.ToEdges(), Allocator.Temp);
            using var neighbors = new NativeMultiHashMap<Id<Point>, Id<Point>>(capacity: pointsLength * pointsLength, Allocator.Temp);

            foreach (var (id, (a, b)) in edges.IdsValues)
            {
                neighbors.Add(b, a);
                neighbors.Add(a, b);
            }

            foreach (var p in points)
            {
                var n = neighbors.GetValuesForKey(p.Id);
                using var list = new NativeList<Id<Point>>(Allocator.Temp);
                foreach (var i in n)
                {
                    list.Add(i);
                }

                for (int i = 0; i < list.Length; i++)
                {
                    for (int j = i + 1; j < list.Length; j++)
                    {
                        var prev = list[j];
                        var curr = p.Id;
                        var next = list[i];
                        stencils.Add(new(prev, curr, next));
                    }
                }
            }

            return stencils;
        }
    }
}
