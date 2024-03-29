using andywiecko.BurstCollections;
using andywiecko.BurstMathUtils;
using andywiecko.PBD2D.Core;
using andywiecko.PBD2D.Systems;
using NUnit.Framework;
using System.Linq;
using Unity.Collections;
using Unity.Mathematics;

namespace andywiecko.PBD2D.Editor.Tests
{
    public class ShapeMatchingConstraintSystemEditorTests
    {
        private class FakeShapeMatchingConstraint : TestComponent, IShapeMatchingConstraint
        {
            private const int PointsCount = 3;
            private const Allocator DataAllocator = Allocator.Persistent;

            public float Stiffness { get; set; } = 1;
            public float Beta { get; set; } = 0;
            public float TotalMass { get; }
            public Ref<NativeIndexedArray<Id<Point>, float2>> Positions { get; } = new NativeIndexedArray<Id<Point>, float2>(PointsCount, DataAllocator);
            public Ref<NativeReference<float2>> CenterOfMass { get; } = new NativeReference<float2>(DataAllocator);
            public Ref<NativeReference<float2x2>> ApqMatrix { get; } = new NativeReference<float2x2>(DataAllocator);
            public Ref<NativeReference<float2x2>> AqqMatrix { get; } = new NativeReference<float2x2>(DataAllocator);
            public Ref<NativeReference<float2x2>> AMatrix { get; } = new NativeReference<float2x2>(float2x2.identity, DataAllocator);
            public Ref<NativeReference<Complex>> Rotation { get; } = new NativeReference<Complex>(Complex.Identity, DataAllocator);
            public Ref<NativeIndexedArray<Id<Point>, float>> Weights { get; } = new NativeIndexedArray<Id<Point>, float>(new[] { 1f, 1f, 1f }, DataAllocator);
            public Ref<NativeList<PointShapeMatchingConstraint>> Constraints { get; } = new(new(PointsCount, DataAllocator));

            public FakeShapeMatchingConstraint(float2[] positions)
            {
                var points = new[] { (Point)0, (Point)1, (Point)2 };
                TotalMass = ShapeMatchingUtils.CalculateTotalMass(points, Weights.Value);
                var com = CenterOfMass.Value.Value = ShapeMatchingUtils.CalculateCenterOfMass<Point>(points, positions, Weights.Value, TotalMass);
                ShapeMatchingUtils.GenerateConstraints(Constraints.Value, points, positions, com);
                AqqMatrix.Value.Value = ShapeMatchingUtils.CalculateAqqMatrix(Constraints.Value.AsReadOnlySpan(), Weights.Value);

                SetPositions(positions);
            }

            public override void Dispose()
            {
                base.Dispose();
                Positions?.Dispose();
                Constraints?.Dispose();
                CenterOfMass?.Dispose();
                ApqMatrix?.Dispose();
                AqqMatrix?.Dispose();
                AMatrix?.Dispose();
                Rotation?.Dispose();
                Weights?.Dispose();
            }

            public void SetPositions(float2[] positions) => Positions.Value.GetInnerArray().CopyFrom(positions);
        }

        private float2[] Positions => component.Positions.Value.GetInnerArray().ToArray();

        private FakeWorld world;
        private ShapeMatchingConstraintSystem system;
        private FakeShapeMatchingConstraint component;

        [SetUp]
        public void SetUp()
        {
            system = new() { World = world = new FakeWorld() };
        }
        [TearDown] public void TearDown() => component?.Dispose();

        [Test]
        public void StationaryTest()
        {
            float2[] initialPositions = { new(0, 0), new(1, 0), new(0, 1) };
            component = new(initialPositions);

            Run();

            Assert.That(Positions, Is.EqualTo(initialPositions));
        }

        [Test]
        public void TranslationTest()
        {
            float2[] initialPositions = { new(0, 0), new(1, 0), new(0, 1) };
            component = new(initialPositions);

            var expectedPositions = initialPositions.Select(i => i + 0.1f).ToArray();
            component.SetPositions(expectedPositions);
            Run();

            Assert.That(Positions, Is.EqualTo(expectedPositions).Using(Float2Comparer.Instance));
        }

        [Test]
        public void RotationTest()
        {
            float2[] initialPositions = { new(0, 0), new(1, 0), new(0, 1) };
            component = new(initialPositions);

            float2[] expectedPositions = { new(0, 0), new(0, -1), new(1, 0) };
            component.SetPositions(expectedPositions);
            Run();

            Assert.That(Positions, Is.EqualTo(expectedPositions).Using(Float2Comparer.Instance));
        }

        [Test]
        public void TransformationTest()
        {
            float2[] initialPositions = { new(0, 0), new(1, 0), new(0, 1) };
            component = new(initialPositions);

            var R = Complex.PolarUnit(phi: 0.6343f * math.PI);
            var t = math.float2(3, 4);
            var expectedPositions = initialPositions.Select(i => (R * i).Value + t).ToArray();
            component.SetPositions(expectedPositions);
            Run();

            Assert.That(Positions, Is.EqualTo(expectedPositions).Using(Float2Comparer.Instance));
        }

        [Test]
        public void DeformationTest()
        {
            float2[] initialPositions = { new(0, 0), new(1, 0), new(0, 1) };
            component = new(initialPositions);

            var dx = (float2)0.1f;
            float2[] positions = { dx, new(1, 0), new(0, 1) };
            component.SetPositions(positions);
            Run();

            float2[] expectedPositions = initialPositions.Select(i => i + dx / 3).ToArray();
            Assert.That(Positions, Is.EqualTo(expectedPositions).Using(Float2Comparer.Instance));
        }

        [Test]
        public void NonZeroDeformationStiffnessTest()
        {
            var beta = 0.95f;
            float2[] initialPositions = { new(0, 0), new(1, 0), new(0, 1) };
            component = new(initialPositions);
            component.Beta = beta;

            var dx = (float2)0.1f;
            float2[] positions = { dx, new(1, 0), new(0, 1) };
            component.SetPositions(positions);
            Run();

            float2[] expectedPositions = initialPositions.Select(i => i + (1 - beta) * dx / 3).ToArray();
            expectedPositions[0] = 0.1f - 2 / 3f * (1 - beta) * dx;
            Assert.That(Positions, Is.EqualTo(expectedPositions).Using(Float2Comparer.Instance));
        }

        private void Run()
        {
            world.ComponentsRegistry.Add(component);
            system.Run();
        }
    }
}
