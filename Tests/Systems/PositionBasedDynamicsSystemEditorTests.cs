using andywiecko.BurstCollections;
using andywiecko.PBD2D.Core;
using andywiecko.PBD2D.Systems;
using NUnit.Framework;
using Unity.Collections;
using Unity.Mathematics;

namespace andywiecko.PBD2D.Editor.Tests
{
    public class PositionBasedDynamicsSystemEditorTests : ISimulationConfigurationProvider
    {
        public SimulationConfiguration SimulationConfiguration { get; private set; }

        private class FakePositionBasedDynamics : FreeComponent, IPositionBasedDynamics
        {
            private const int PointsCount = 1;
            private const Allocator DataAllocator = Allocator.Persistent;

            public float2 ExternalForce { get; set; } = 0;
            public float Damping { get; set; } = 0;
            public Ref<NativeIndexedArray<Id<Point>, float2>> Positions { get; } = new NativeIndexedArray<Id<Point>, float2>(PointsCount, DataAllocator);
            public Ref<NativeIndexedArray<Id<Point>, float2>> PredictedPositions { get; } = new NativeIndexedArray<Id<Point>, float2>(PointsCount, DataAllocator);
            public Ref<NativeIndexedArray<Id<Point>, float2>> Velocities { get; } = new NativeIndexedArray<Id<Point>, float2>(PointsCount, DataAllocator);
            public Ref<NativeIndexedArray<Id<Point>, float>> MassesInv { get; } = new NativeIndexedArray<Id<Point>, float>(new[] { 1f }, Allocator.Persistent);

            public override void Dispose()
            {
                base.Dispose();

                Positions?.Dispose();
                PredictedPositions?.Dispose();
                Velocities?.Dispose();
                MassesInv?.Dispose();
            }
        }

        private float2 Velocity
        {
            get => component.Velocities.Value[Id<Point>.Zero];
            set => component.Velocities.Value[Id<Point>.Zero] = value;
        }
        private float2 Position
        {
            get => component.Positions.Value[Id<Point>.Zero];
            set => component.Positions.Value[Id<Point>.Zero] = value;
        }
        private float2 PredictedPosition
        {
            get => component.PredictedPositions.Value[Id<Point>.Zero];
            set => component.PredictedPositions.Value[Id<Point>.Zero] = value;
        }

        private PositionBasedDynamicsStepStartSystem startSystem;
        private PositionBasedDynamicsStepEndSystem endSystem;
        private FakePositionBasedDynamics component;

        [SetUp]
        public void SetUp()
        {
            TestUtils.New(ref startSystem);
            TestUtils.New(ref endSystem);
            //startSystem.ConfigurationProvider = this;
            //endSystem.ConfigurationProvider = this;

            component = new();

            SimulationConfiguration = new()
            {
                DeltaTime = 1e-9f,
                GlobalDamping = 0,
                GlobalExternalForce = 0
            };
        }

        [TearDown]
        public void TearDown()
        {
            component?.Dispose();
        }

        [Test]
        public void StepStartTest()
        {
            component.ExternalForce = new(0, -10);
            SimulationConfiguration.DeltaTime = 1;
            Position = new(0, 10);

            startSystem.Schedule().Complete();

            Assert.That(Velocity, Is.EqualTo(math.float2(0, -10)));
            Assert.That(PredictedPosition, Is.EqualTo(float2.zero));
        }

        [Test]
        public void StepEndTest()
        {
            component.ExternalForce = new(0, -10);
            SimulationConfiguration.DeltaTime = 1;
            Position = new(0, 10);
            PredictedPosition = new(0, 8);

            endSystem.Schedule().Complete();

            Assert.That(Position, Is.EqualTo(PredictedPosition));
            Assert.That(Velocity, Is.EqualTo(math.float2(0, -2)));
        }

        [Test]
        public void ProjectileTest()
        {
            float2 x0 = new(5, -10);
            float2 v0 = new(1, 0);
            float2 a = new(0, -1);
            var t = 15f;
            component.ExternalForce = a;
            SimulationConfiguration.DeltaTime = 1e-2f;
            Position = x0;
            PredictedPosition = x0;
            Velocity = v0;

            SimulateFor(seconds: t);

            var x = ExpectedPosition(x0, v0, a, t);
            var v = ExpectedVelocity(v0, a, t);
            Assert.That(Position, Is.EqualTo(x).Using(new Float2Comparer(epsilon: 1f)));
            Assert.That(Velocity, Is.EqualTo(v).Using(new Float2Comparer(epsilon: 1f)));
        }

        private float2 ExpectedPosition(float2 x0, float2 v0, float2 a, float t) => x0 + v0 * t + 0.5f * a * t * t;
        private float2 ExpectedVelocity(float2 v0, float2 a, float t) => v0 + a * t;

        private void SimulateFor(float seconds)
        {
            var t = 0f;
            var dt = SimulationConfiguration.DeltaTime;
            while (t < seconds)
            {
                startSystem.Schedule().Complete();
                endSystem.Schedule().Complete();
                t += dt;
            }
        }
    }
}