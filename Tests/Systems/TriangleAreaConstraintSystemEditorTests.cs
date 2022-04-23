using andywiecko.BurstCollections;
using andywiecko.BurstMathUtils;
using andywiecko.PBD2D.Core;
using andywiecko.PBD2D.Systems;
using NUnit.Framework;
using Unity.Collections;
using Unity.Mathematics;

namespace andywiecko.PBD2D.Editor.Tests
{
    public class TriangleAreaConstraintSystemEditorTests
    {
        private class FakeTriangleAreaConstraint : TestComponent, ITriangleAreaConstraint
        {
            private const Allocator DataAllocator = Allocator.Persistent;
            private const int PointsCount = 3;

            public float Stiffness { get; set; } = 1;
            public Ref<NativeIndexedArray<Id<Point>, float2>> PredictedPositions { get; } = new NativeIndexedArray<Id<Point>, float2>(PointsCount, DataAllocator);
            public Ref<NativeIndexedArray<Id<Point>, float>> MassesInv { get; } = new NativeIndexedArray<Id<Point>, float>(new[] { 1f, 1f, 1f }, DataAllocator);
            public Ref<NativeList<TriangleAreaConstraint>> Constraints { get; } = new NativeList<TriangleAreaConstraint>(64, DataAllocator);

            public override void Dispose()
            {
                base.Dispose();
                PredictedPositions?.Dispose();
                MassesInv?.Dispose();
                Constraints?.Dispose();
            }

            public FakeTriangleAreaConstraint(float2[] positions)
            {
                SetPositions(positions);
                Constraints.Value.Add((0, 1, 2, GetArea2()));
            }

            public void SetPositions(float2[] positions)
            {
                PredictedPositions.Value.GetInnerArray().CopyFrom(positions);
            }

            public void SetRestArea2(float area2)
            {
                Constraints.Value[default] = Constraints.Value[default].With(area2);
            }

            public float GetArea2()
            {
                var p = PredictedPositions.Value.AsReadOnly();
                return MathUtils.TriangleSignedArea2(p[(Id<Point>)0], p[(Id<Point>)1], p[(Id<Point>)2]);
            }
        }

        private float2[] Positions => component.PredictedPositions.Value.GetInnerArray().ToArray();

        private FakeWorld world;
        private TriangleAreaConstraintSystem system;
        private FakeTriangleAreaConstraint component;

        [SetUp]
        public void SetUp()
        {
            TestUtils.New(ref system);
            system.World = world = new();
        }

        [TearDown]
        public void TearDown()
        {
            component?.Dispose();
        }

        [Test]
        public void StationaryTest()
        {
            float2[] initialPositions = { new(0, 0), new(1, 0), new(0, 1) };
            component = new(initialPositions);
            world.ComponentsRegistry.Add(component);
            system.Schedule().Complete();
            Assert.That(Positions, Is.EqualTo(initialPositions));
        }

        [Test]
        public void RightTriangleTest()
        {
            float2[] initialPositions = { new(0, 0), new(1, 0), new(0, 1) };
            component = new(initialPositions);
            world.ComponentsRegistry.Add(component);
            var initialArea2 = component.GetArea2();
            Assert.That(initialArea2, Is.EqualTo(1));

            float2[] positions = { new(0, 0), new(2, 0), new(0, 2) };
            component.SetPositions(positions);
            foreach (var _ in 0..5)
            {
                system.Schedule().Complete();
            }

            Assert.That(component.GetArea2(), Is.EqualTo(initialArea2));
        }

        [Test]
        public void EquilateralTriangleTest()
        {
            float2[] initialPositions = { new(0, 0), new(1, 0), new(0.5f, math.sqrt(3) / 2) };
            component = new(initialPositions);
            world.ComponentsRegistry.Add(component);
            var expectedArea2 = math.sqrt(3) / 8;
            component.SetRestArea2(expectedArea2);

            foreach (var _ in 0..5)
            {
                system.Schedule().Complete();
            }

            float2[] expectedPositions =
            {
                new(0.25f, math.sqrt(3) / 12),
                new(0.75f, math.sqrt(3) / 12),
                new(0.50f, math.sqrt(3) / 3),
            };
            Assert.That(component.GetArea2(), Is.EqualTo(expectedArea2).Within(1e-6f));
            Assert.That(Positions, Is.EqualTo(expectedPositions).Using(Float2Comparer.Instance));
        }
    }
}
