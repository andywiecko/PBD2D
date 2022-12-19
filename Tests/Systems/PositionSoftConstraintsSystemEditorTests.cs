using andywiecko.BurstCollections;
using andywiecko.PBD2D.Core;
using andywiecko.PBD2D.Systems;
using NUnit.Framework;
using Unity.Collections;
using Unity.Mathematics;

namespace andywiecko.PBD2D.Editor.Tests
{
    public class PositionSoftConstraintsSystemEditorTests
    {
        private class FakeComponent : TestComponent, IPositionSoftConstraints
        {
            public float Stiffness { get; set; } = 1;
            public float Compliance { get; set; } = 0;

            public float2 Position { get => Positions.Value[default]; set => Positions.Value[default] = value; }
            public float Weight { get => Weights.Value[default]; set => Weights.Value[default] = value; }

            public Ref<NativeIndexedArray<Id<Point>, float2>> Positions { get; } = new(new(1, Allocator.Persistent));
            public Ref<NativeIndexedArray<Id<Point>, float>> Weights { get; } = new(new(1, Allocator.Persistent));
            public Ref<NativeList<PositionConstraint>> Constraints { get; } = new(new(64, Allocator.Persistent));

            public override void Dispose()
            {
                Positions?.Dispose();
                Weights?.Dispose();
                Constraints?.Dispose();
            }
        }

        private FakeComponent component;
        private PositionSoftConstraintsSystem system;

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

        [Test]
        public void IdentityTest()
        {
            var initialPosition = component.Position = new(1, 1);
            component.Weight = 1;
            component.Constraints.Value.Clear();

            system.Run();

            Assert.That(component.Position, Is.EqualTo(initialPosition));
        }

        [Test]
        public void PositionConstraintStiffnessTest([Values(0, 0.1f, 0.5f, 1f)] float stiffness)
        {
            var initialPosition = component.Position = new(1, 1);
            component.Weight = 1;
            component.Stiffness = stiffness;
            component.Constraints.Value.Add(new(default, float2.zero));

            system.Run();

            Assert.That(component.Position, Is.EqualTo((1 - stiffness) * initialPosition));
        }

        [Test]
        public void PositionConstraintComplianceTest([Values(0, 1, 9, 99, 999)] float compliance)
        {
            var initialPosition = component.Position = new(1, 1);
            component.Weight = 1;
            var a = component.Compliance = compliance;
            component.Constraints.Value.Add(new(default, float2.zero));
            var config = system.World.ConfigurationsRegistry.Get<PBDConfiguration>();
            config.DeltaTime = 1;
            config.StepsCount = 1;

            system.Run();

            Assert.That(component.Position, Is.EqualTo(a / (1 + a) * initialPosition));
        }

        [Test]
        public void DoNotMoveWeight0PointsTest([Values(0, 0.1f, 0.5f, 1f)] float stiffness)
        {
            var initialPosition = component.Position = new(1, 1);
            component.Weight = 0;
            component.Stiffness = stiffness;
            component.Constraints.Value.Add(new(default, float2.zero));

            system.Run();

            Assert.That(component.Position, Is.EqualTo(initialPosition));
        }
    }
}