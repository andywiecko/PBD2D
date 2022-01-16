using andywiecko.BurstCollections;
using andywiecko.PBD2D.Core;
using andywiecko.PBD2D.Systems;
using NUnit.Framework;
using System.Linq;
using Unity.Collections;
using Unity.Mathematics;

namespace andywiecko.PBD2D.Editor.Tests
{
    public class EdgeLengthConstraintSystemEditorTests
    {
        private class FakeEdgeLengthConstraint : FreeComponent, IEdgeLengthConstraint
        {
            private const Allocator DataAllocator = Allocator.Persistent;
            private const int PointsCount = 5;
            private const int EdgesCount = 1;

            public float Stiffness { get; set; } = 1;
            public Ref<NativeIndexedArray<Id<Point>, float2>> PredictedPositions { get; } = new NativeIndexedArray<Id<Point>, float2>(PointsCount, DataAllocator);
            public NativeIndexedArray<Id<Point>, float>.ReadOnly MassesInv => massesInv.Value.AsReadOnly();
            public NativeIndexedArray<Id<Edge>, Edge>.ReadOnly Edges => edges.Value.AsReadOnly();
            public NativeIndexedArray<Id<Edge>, float>.ReadOnly RestLengths => restLengths.Value.AsReadOnly();

            private Ref<NativeIndexedArray<Id<Point>, float>> massesInv = new NativeIndexedArray<Id<Point>, float>(PointsCount, DataAllocator);
            private Ref<NativeIndexedArray<Id<Edge>, Edge>> edges = new NativeIndexedArray<Id<Edge>, Edge>(EdgesCount, DataAllocator);
            private Ref<NativeIndexedArray<Id<Edge>, float>> restLengths = new NativeIndexedArray<Id<Edge>, float>(EdgesCount, DataAllocator);

            public override void Dispose()
            {
                base.Dispose();
                PredictedPositions?.Dispose();
                massesInv?.Dispose();
                edges?.Dispose();
                restLengths?.Dispose();
            }

            public FakeEdgeLengthConstraint SetMassInv(int i, float mInv)
            {
                massesInv.Value[(Id<Point>)i] = mInv;
                return this;
            }

            public FakeEdgeLengthConstraint SetPosition(int i, float2 pi)
            {
                PredictedPositions.Value[(Id<Point>)i] = pi;
                return this;
            }

            public FakeEdgeLengthConstraint SetPositions(params float2[] p)
            {
                for (int i = 0; i < p.Length; i++)
                {
                    SetPosition(i, p[i]);
                }
                return this;
            }

            public FakeEdgeLengthConstraint SetEdge(int i, int j, float restLength)
            {
                edges.Value[Id<Edge>.Zero] = (i, j);
                restLengths.Value[Id<Edge>.Zero] = restLength;
                return this;
            }

            public FakeEdgeLengthConstraint DefaultConfiguration()
            {
                return
                    SetPositions(defaultInitialPositions)
                    .SetEdge(1, 2, restLength: 1f)
                    .SetMassInv(1, 1f)
                    .SetMassInv(2, 1f)
                ;
            }
        }

        private static readonly float2[] defaultInitialPositions = new float2[]
        {
            new(0, 0),
            new(1, 0),
            new(2, 0),
            new(3, 0),
            new(4, 0)
        };

        private float2[] Positions => component.PredictedPositions.Value.GetInnerArray().ToArray();

        private EdgeLengthConstraintSystem system;
        private FakeEdgeLengthConstraint component;

        [SetUp]
        public void SetUp()
        {
            TestUtils.New(ref system);
            component = new();
        }

        [TearDown]
        public void TearDown()
        {
            component?.Dispose();
        }

        [Test]
        public void StationaryTest()
        {
            component
                .DefaultConfiguration()
                .SetPosition(1, new(1, 0))
                .SetPosition(2, new(2, 0))
            ;
            system.Schedule().Complete();
            Assert.That(Positions, Is.EqualTo(defaultInitialPositions));
        }

        [Test]
        public void StretchingTest()
        {
            component
                .DefaultConfiguration()
                .SetPosition(1, new(0.9f, 0))
                .SetPosition(2, new(2.1f, 0))
            ;
            system.Schedule().Complete();
            Assert.That(Positions, Is.EqualTo(defaultInitialPositions).Using(Float2Comparer.Instance));
        }

        [Test]
        public void CompressionTest()
        {
            component
                .DefaultConfiguration()
                .SetPosition(1, new(1.1f, 0))
                .SetPosition(2, new(1.9f, 0))
            ;
            system.Schedule().Complete();
            Assert.That(Positions, Is.EqualTo(defaultInitialPositions).Using(Float2Comparer.Instance));
        }

        [Test]
        public void NonUniformMassTest()
        {
            component
                .DefaultConfiguration()
                .SetPosition(1, new(1.1f, 0))
                .SetPosition(2, new(1.9f, 0))
                .SetMassInv(1, 0f)
            ;
            system.Schedule().Complete();
            var expectedPositions = defaultInitialPositions.ToArray();
            expectedPositions[1] = new(1.1f, 0);
            expectedPositions[2] = new(2.1f, 0);
            Assert.That(Positions, Is.EqualTo(expectedPositions).Using(Float2Comparer.Instance));
        }

        [Test]
        public void NonUnitStiffnessTest()
        {
            component
                .DefaultConfiguration()
                .SetPosition(1, new(1.1f, 0))
                .SetPosition(2, new(1.9f, 0))
                .Stiffness = 0.5f
            ;
            system.Schedule().Complete();
            var expectedPositions = defaultInitialPositions.ToArray();
            expectedPositions[1] = new(1.05f, 0);
            expectedPositions[2] = new(1.95f, 0);
            Assert.That(Positions, Is.EqualTo(expectedPositions).Using(Float2Comparer.Instance));
        }
    }
}