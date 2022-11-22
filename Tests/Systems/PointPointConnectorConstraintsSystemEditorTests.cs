using andywiecko.BurstCollections;
using andywiecko.PBD2D.Core;
using andywiecko.PBD2D.Systems;
using NUnit.Framework;
using System;
using Unity.Collections;
using Unity.Mathematics;

namespace andywiecko.PBD2D.Editor.Tests
{
    public class PointPointConnectorConstraintsSystemEditorTests
    {
        private class FakeComponent : TestComponent, IPointPointConnectorConstraints
        {
            private class FakePointsProvider : IPointsProvider, IDisposable
            {
                public Ref<NativeArray<Point>> Points { get; } = new(new(1, Allocator.Persistent));
                public Ref<NativeIndexedArray<Id<Point>, float2>> Positions { get; } = new(new(1, Allocator.Persistent));
                public Ref<NativeIndexedArray<Id<Point>, float>> Weights { get; } = new(new(1, Allocator.Persistent));

                public void Dispose()
                {
                    Points?.Dispose();
                    Positions?.Dispose();
                    Weights?.Dispose();
                }
            }

            public float2 PositionA { get => connectee.Positions.Value[default]; set => connectee.Positions.Value[default] = value; }
            public float2 PositionB { get => connecter.Positions.Value[default]; set => connecter.Positions.Value[default] = value; }
            public float WeightA { get => connectee.Weights.Value[default]; set => connectee.Weights.Value[default] = value; }
            public float WeightB { get => connecter.Weights.Value[default]; set => connecter.Weights.Value[default] = value; }

            public float Stiffness { get; set; } = 1;
            public float Compliance { get; set; } = 0;
            public float Weight { get; set; } = 0.5f;
            IPointsProvider IPointPointConnectorConstraints.Connectee => connectee;
            IPointsProvider IPointPointConnectorConstraints.Connecter => connecter;

            private readonly FakePointsProvider connectee = new();
            private readonly FakePointsProvider connecter = new();

            public Ref<NativeList<PointPointConnectorConstraint>> Constraints { get; } = new(new(Allocator.Persistent));

            public override void Dispose()
            {
                Constraints?.Dispose();
                connectee?.Dispose();
                connecter?.Dispose();
            }
        }

        private FakeComponent component;
        private PointPointConnectorConstraintsSystem system;

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
        public void SymmetricCaseTest()
        {
            component.PositionA = 0;
            component.PositionB = 1;
            component.WeightA = component.WeightB = 1;
            component.Constraints.Value.Add(new());

            system.Run();

            Assert.That(component.PositionA, Is.EqualTo(math.float2(0.5f)));
            Assert.That(component.PositionB, Is.EqualTo(component.PositionA));
        }

        [Test]
        public void StiffnessTest([Values(0, 0.1f, 0.5f, 0.9f, 1)] float stiffness)
        {
            component.PositionA = 0;
            component.PositionB = 1;
            component.WeightA = component.WeightB = 1;
            component.Constraints.Value.Add(new());
            component.Stiffness = stiffness;

            system.Run();

            var expected = 0.5f * math.float2(stiffness);
            Assert.That(component.PositionA, Is.EqualTo(expected));
            Assert.That(component.PositionB, Is.EqualTo(1 - expected));
        }

        [Test]
        public void PointWeightsTest()
        {
            component.PositionA = 0;
            component.PositionB = 1;
            component.WeightA = 0.25f;
            component.WeightB = 0.75f;
            component.Constraints.Value.Add(new());

            system.Run();

            Assert.That(component.PositionA, Is.EqualTo(math.float2(0.25f)));
            Assert.That(component.PositionB, Is.EqualTo(component.PositionA));
        }

        [Test]
        public void ComplianceTest([Values(0, 2, 4, 8, 98)] float compliance)
        {
            var config = system.World.ConfigurationsRegistry.Get<PBDConfiguration>();
            config.DeltaTime = 1;
            config.StepsCount = 1;
            component.PositionA = 0;
            component.PositionB = 1;
            component.WeightA = component.WeightB = 1;
            component.Constraints.Value.Add(new());
            var a = component.Compliance = compliance;

            system.Run();

            var expected = (float2)1 / (a + 2);
            Assert.That(component.PositionA, Is.EqualTo(expected));
            Assert.That(component.PositionB, Is.EqualTo(1 - expected));
        }

        [Test]
        public void ConstraintWeightTest()
        {
            component.PositionA = 0;
            component.PositionB = 1;
            component.WeightA = 1;
            component.WeightB = 1;
            component.Weight = 0.25f;
            component.Constraints.Value.Add(new());

            system.Run();

            Assert.That(component.PositionA, Is.EqualTo(math.float2(0.25f)));
            Assert.That(component.PositionB, Is.EqualTo(component.PositionA));
        }
    }
}