using andywiecko.BurstCollections;
using andywiecko.BurstMathUtils;
using andywiecko.PBD2D.Core;
using andywiecko.PBD2D.Systems;
using NUnit.Framework;
using Unity.Collections;
using Unity.Mathematics;

namespace andywiecko.PBD2D.Editor.Tests
{
    public class PointLineCollisionSystemEditorTests
    {
        private class FakePoint : TestComponent, IPointCollideWithLine
        {
            public AABB Bounds { get; set; }
            public float CollisionRadius { get; set; } = 0;
            public Ref<NativeIndexedArray<Id<Point>, float2>> PredictedPositions { get; } = new NativeIndexedArray<Id<Point>, float2>(1, Allocator.Persistent);
            public Ref<NativeIndexedArray<Id<Point>, float2>> Positions { get; } = new NativeIndexedArray<Id<Point>, float2>(1, Allocator.Persistent);
            public Ref<NativeIndexedArray<Id<Point>, float2>> Velocities { get; } = new NativeIndexedArray<Id<Point>, float2>(1, Allocator.Persistent);
            public float Friction => throw new System.NotImplementedException();
            public ref float2 PredictedPosition => ref PredictedPositions.Value.ElementAt(default);
            public ref float2 Position => ref Positions.Value.ElementAt(default);
            public ref float2 Velocity => ref Velocities.Value.ElementAt(default);

            public override void Dispose()
            {
                PredictedPositions?.Dispose();
                Positions?.Dispose();
                Velocities?.Dispose();
            }
        }

        private class FakeLine : TestComponent, ILineCollideWithPoint
        {
            public Line Line { get; set; }
            public float2 Displacement { get; set; }
        }

        private class FakeTuple : TestComponent, IPointLineCollisionTuple
        {
            public float Friction { get; set; }
            public IPointCollideWithLine PointComponent { get; set; }
            public ILineCollideWithPoint LineComponent { get; set; }
        }

        private FakePoint point;
        private FakeLine line;
        private FakeTuple tuple;
        private PointLineCollisionSystem system;

        [SetUp]
        public void SetUp()
        {
            TestUtils.New(ref system);
            point = new();
            line = new();
            tuple = new() { PointComponent = point, LineComponent = line };
            system.World = new FakeWorld(point, line, tuple);
        }

        [TearDown]
        public void TearDown()
        {
            point?.Dispose();
        }

        [Test]
        public void PointBelowLineTest()
        {
            tuple.Friction = 0;
            point.Position = new(1, -1);
            point.PredictedPosition = new(1, -1);

            line.Displacement = 0;
            line.Line = new(point: default, normal: MathUtils.Up());

            system.Run();

            Assert.That(point.PredictedPosition, Is.EqualTo(math.float2(1, 0)));
        }

        [Test]
        public void PointAboveLineTest()
        {
            tuple.Friction = 0;
            point.Position = new(1, -1);
            var initial = point.PredictedPosition = new(1, -1);

            line.Displacement = 0;
            line.Line = new(point: default, normal: MathUtils.Right());

            system.Run();

            Assert.That(point.PredictedPosition, Is.EqualTo(initial));
        }

        [Test]
        public void SmallFrictionTest()
        {
            tuple.Friction = 1;
            point.Position = new(-10, 0);
            point.PredictedPosition = new(1, -1);

            line.Displacement = 0;
            line.Line = new(point: default, normal: MathUtils.Up());

            system.Run();

            Assert.That(point.PredictedPosition, Is.EqualTo(math.float2(0, 0)));
        }

        [Test]
        public void HighFrictionTest()
        {
            tuple.Friction = 100_000;
            var initial = point.Position = new(-10, 0);
            point.PredictedPosition = new(1, -1);

            line.Displacement = 0;
            line.Line = new(point: default, normal: MathUtils.Up());

            system.Run();

            Assert.That(point.PredictedPosition, Is.EqualTo(initial));
        }

        [Test]
        public void FrictionLineDisplacementTest()
        {
            tuple.Friction = 1;
            point.Position = new(0, 0);
            point.PredictedPosition = new(0, -1);

            line.Displacement = 10 * MathUtils.Right();
            line.Line = new(point: default, normal: MathUtils.Up());

            system.Run();

            Assert.That(point.PredictedPosition, Is.EqualTo(math.float2(1, 0)));
        }

        [Test]
        public void BoundsTest()
        {
            tuple.Friction = 0;
            point.Bounds = new(1, 1);
            point.Position = new(1, -1);
            var initial = point.PredictedPosition = new(1, -1);

            line.Displacement = 0;
            line.Line = new(point: default, normal: MathUtils.Up());

            system.Run();

            Assert.That(point.PredictedPosition, Is.EqualTo(initial));
        }
    }
}