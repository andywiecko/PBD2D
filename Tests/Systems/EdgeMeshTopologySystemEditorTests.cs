using andywiecko.BurstCollections;
using andywiecko.PBD2D.Core;
using andywiecko.PBD2D.Systems;
using NUnit.Framework;
using Unity.Collections;
using Unity.Collections.NotBurstCompatible;

namespace andywiecko.PBD2D.Editor.Tests
{
    public class EdgeMeshTopologySystemEditorTests
    {
        private class FakeComponent : TestComponent, IEdgeMeshTopology
        {
            public Point[] Points { get => points.Value.ToArray(); set => points.Value.CopyFromNBC(value); }
            Ref<NativeList<Point>> IEdgeMeshTopology.Points => points;
            private readonly Ref<NativeList<Point>> points = new(new(64, Allocator.Persistent));

            public Edge[] Edges { get => edges.Value.ToArray(); set => edges.Value.CopyFromNBC(value); }
            Ref<NativeList<Edge>> IEdgeMeshTopology.Edges => edges;
            private readonly Ref<NativeList<Edge>> edges = new(new(64, Allocator.Persistent));

            public void Remove(Point p) => pointsToRemove.Value.Add(p);
            Ref<NativeHashSet<Point>> IEdgeMeshTopology.PointsToRemove => pointsToRemove;
            private readonly Ref<NativeHashSet<Point>> pointsToRemove = new(new(64, Allocator.Persistent));

            public Point[] RecentlyRemovedPoints => recentlyRemovedPoints.Value.ToArray();
            Ref<NativeHashSet<Point>> IEdgeMeshTopology.RecentlyRemovedPoints => recentlyRemovedPoints;
            private readonly Ref<NativeHashSet<Point>> recentlyRemovedPoints = new(new(64, Allocator.Persistent));

            public override void Dispose()
            {
                points?.Dispose();
                edges?.Dispose();
                pointsToRemove?.Dispose();
                recentlyRemovedPoints?.Dispose();
            }
        }

        private EdgeMeshTopologySystem system;
        private FakeComponent component;

        [SetUp]
        public void SetUp()
        {
            component = new();
            system = new() { World = new FakeWorld(component) };
        }

        [TearDown]
        public void TearDown()
        {
            component?.Dispose();
        }

        static Edge e(int i, int j) => new(new(i), new(j));
        private static readonly TestCaseData[] topologyRemovedPointsTest =
        {
            new (new Point[] { }, new Edge[] { }, new Point[] { })
            {
                TestName = "Test case 1 (identity)",
                ExpectedResult = new Point[] { }
            },
            new (new Point[] { new(0), new(1), new(2) }, new Edge[] { }, new Point[] { new(0), new(1) })
            {
                TestName = "Test case 2 (remove some points)",
                ExpectedResult = new Point[] { new(0), new(1) }
            },
            new (new Point[] { new(0), new(1), new(2) }, new Edge[] { }, new Point[] { new(0), new(1), new(2) })
            {
                TestName = "Test case 3 (remove all points)",
                ExpectedResult = new Point[] { new(0), new(1), new(2) }
            },
        };

        [Test, TestCaseSource(nameof(topologyRemovedPointsTest))]
        public Point[] TopologyRemovedPointsTest(Point[] points, Edge[] edges, Point[] pointsToRemove)
        {
            component.Points = points;
            component.Edges = edges;
            foreach (var p in pointsToRemove)
            {
                component.Remove(p);
            }

            system.Run();

            return component.RecentlyRemovedPoints;
        }

        private static readonly TestCaseData[] topologyPointsTest =
        {
            new (new Point[] { }, new Edge[] { }, new Point[] { })
            {
                TestName = "Test case 1 (identity)",
                ExpectedResult = new Point[] { }
            },
            new (new Point[] { new(0), new(1) }, new Edge[] { }, new Point[] { new(0) })
            {
                TestName = "Test case 2 (remove some points)",
                ExpectedResult = new Point[] { new(1) }
            },
            new (new Point[] { new(0), new(1) }, new Edge[] { }, new Point[] { new(0), new(1) })
            {
                TestName = "Test case 3 (remove all points)",
                ExpectedResult = new Point[] { }
            }
        };

        [Test, TestCaseSource(nameof(topologyPointsTest))]
        public Point[] TopologyPointsTest(Point[] points, Edge[] edges, Point[] pointsToRemove)
        {
            component.Points = points;
            component.Edges = edges;
            foreach (var p in pointsToRemove)
            {
                component.Remove(p);
            }

            system.Run();

            return component.Points;
        }

        private static readonly TestCaseData[] topologyEdgesTest =
{
            new (new Point[] { }, new Edge[] { e(0, 1) }, new Point[] { })
            {
                TestName = "Test case 1 (identity)",
                ExpectedResult = new Edge[] { e(0, 1) }
            },
            new (new Point[] { }, new Edge[] { e(0, 1), e(2, 3) }, new Point[] { new(0) })
            {
                TestName = "Test case 2 (remove some edges)",
                ExpectedResult = new Edge[] { e(2, 3) }
            },
            new (new Point[] { }, new Edge[] { e(0, 1), e(2, 3) }, new Point[] { new(0), new(3) })
            {
                TestName = "Test case 3 (remove all edges)",
                ExpectedResult = new Edge[] { }
            }
        };

        [Test, TestCaseSource(nameof(topologyEdgesTest))]
        public Edge[] TopologyEdgesTest(Point[] points, Edge[] edges, Point[] pointsToRemove)
        {
            component.Points = points;
            component.Edges = edges;
            foreach (var p in pointsToRemove)
            {
                component.Remove(p);
            }

            system.Run();

            return component.Edges;
        }
    }
}
