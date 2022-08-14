using andywiecko.BurstCollections;
using andywiecko.PBD2D.Core;
using andywiecko.PBD2D.Systems;
using NUnit.Framework;
using Unity.Collections;
using Unity.Mathematics;

namespace andywiecko.PBD2D.Editor.Tests
{
    public class PositionBasedDynamicsSystemEditorTests
    {
        private class FakePositionBasedDynamics : TestComponent, IPositionBasedDynamics
        {
            private const int PointsCount = 1;
            private const Allocator DataAllocator = Allocator.Persistent;

            public float2 ExternalAcceleration { get; set; } = 0;
            public float Damping { get; set; } = 0;
            public Ref<NativeList<Point>> Points { get; } = new NativeList<Point>(PointsCount, DataAllocator) { Length = 1, [default] = new(default) };
            public Ref<NativeIndexedArray<Id<Point>, float2>> PreviousPositions { get; } = new NativeIndexedArray<Id<Point>, float2>(PointsCount, DataAllocator);
            public Ref<NativeIndexedArray<Id<Point>, float2>> Positions { get; } = new NativeIndexedArray<Id<Point>, float2>(PointsCount, DataAllocator);
            public Ref<NativeIndexedArray<Id<Point>, float2>> Velocities { get; } = new NativeIndexedArray<Id<Point>, float2>(PointsCount, DataAllocator);

            public override void Dispose()
            {
                base.Dispose();
                Points?.Dispose();
                PreviousPositions?.Dispose();
                Positions?.Dispose();
                Velocities?.Dispose();
            }
        }

        private float2 Velocity
        {
            get => component.Velocities.Value[Id<Point>.Zero];
            set => component.Velocities.Value[Id<Point>.Zero] = value;
        }
        private float2 PreviousPosition
        {
            get => component.PreviousPositions.Value[Id<Point>.Zero];
            set => component.PreviousPositions.Value[Id<Point>.Zero] = value;
        }
        private float2 Position
        {
            get => component.Positions.Value[Id<Point>.Zero];
            set => component.Positions.Value[Id<Point>.Zero] = value;
        }

        private FakeWorld world;
        private PositionBasedDynamicsStepStartSystem startSystem;
        private PositionBasedDynamicsStepEndSystem endSystem;
        private FakePositionBasedDynamics component;

        [SetUp]
        public void SetUp()
        {
            component = new();

            world = new FakeWorld(component)
            {
                Configuration = {
                    DeltaTime = 1e-9f,
                    GlobalDamping = 0,
                    GlobalExternalAcceleration = 0,
                    StepsCount = 1
                }
            };

            startSystem = new() { World = world };
            endSystem = new() { World = world };
        }

        [TearDown]
        public void TearDown()
        {
            component?.Dispose();
        }

        [Test]
        public void StepStartTest()
        {
            component.ExternalAcceleration = new(0, -10);
            world.Configuration.DeltaTime = 1;
            PreviousPosition = 0;
            Position = new(0, 10);

            startSystem.Run();

            Assert.That(Velocity, Is.EqualTo(math.float2(0, -10)));
            Assert.That(PreviousPosition, Is.EqualTo(math.float2(0, 10)));
            Assert.That(Position, Is.EqualTo(float2.zero));
        }

        [Test]
        public void StepEndTest()
        {
            component.ExternalAcceleration = new(0, -10);
            world.Configuration.DeltaTime = 1;
            PreviousPosition = new(0, 10);
            Position = new(0, 8);

            endSystem.Run();

            Assert.That(Velocity, Is.EqualTo(math.float2(0, -2)));
        }

        [Test]
        public void ProjectileTest()
        {
            float2 x0 = new(5, -10);
            float2 v0 = new(1, 0);
            float2 a = new(0, -1);
            var t = 15f;
            component.ExternalAcceleration = a;
            world.Configuration.DeltaTime = 1e-2f;
            PreviousPosition = x0;
            Position = x0;
            Velocity = v0;

            SimulateFor(seconds: t);

            var x = ExpectedPosition(x0, v0, a, t);
            var v = ExpectedVelocity(v0, a, t);
            Assert.That(PreviousPosition, Is.EqualTo(x).Using(new Float2Comparer(epsilon: 1f)));
            Assert.That(Velocity, Is.EqualTo(v).Using(new Float2Comparer(epsilon: 1f)));
        }

        private float2 ExpectedPosition(float2 x0, float2 v0, float2 a, float t) => x0 + v0 * t + 0.5f * a * t * t;
        private float2 ExpectedVelocity(float2 v0, float2 a, float t) => v0 + a * t;

        private void SimulateFor(float seconds)
        {
            var t = 0f;
            var dt = world.Configuration.DeltaTime;
            while (t < seconds)
            {
                startSystem.Run();
                endSystem.Run();
                t += dt;
            }
        }
    }
}