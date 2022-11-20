using andywiecko.BurstCollections;
using andywiecko.PBD2D.Core;
using andywiecko.PBD2D.Systems;
using NUnit.Framework;
using Unity.Collections;
using Unity.Mathematics;

namespace andywiecko.PBD2D.Editor.Tests
{
    public class RegeneratePositionConstraintsSystemEditorTests
    {
        private class FakeComponent : TestComponent, IRegeneratePositionConstraints
        {
            public PositionConstraint Constraint { get => Constraints.Value[default]; set => Constraints.Value[default] = value; }
            public float2 InitialRelativePosition { get => InitialRelativePositions.Value[default]; set => InitialRelativePositions.Value[default] = value; }

            public Ref<NativeList<PositionConstraint>> Constraints { get; } = new(new(1, Allocator.Persistent) { Length = 1 });
            public Ref<NativeList<float2>> InitialRelativePositions { get; } = new(new(1, Allocator.Persistent) { Length = 1 });
            public float2 TransformPosition { get; set; }
            public bool TransformChanged { get; set; }

            public override void Dispose()
            {
                Constraints?.Dispose();
                InitialRelativePositions?.Dispose();
            }
        }

        private FakeComponent component;
        private RegeneratePositionConstraintsSystem system;

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
        public void RegenerateTest()
        {
            component.Constraint = new(id: default, position: 1);
            component.InitialRelativePosition = 1;
            component.TransformPosition = 1;
            component.TransformChanged = true;

            system.Run();

            var expected = new PositionConstraint(id: default, position: 2);
            Assert.That(component.Constraint, Is.EqualTo(expected));
        }

        [Test]
        public void DoNotRegenerateTest()
        {
            component.Constraint = new(id: default, position: 1);
            component.InitialRelativePosition = 1;
            component.TransformPosition = 1;
            component.TransformChanged = false;

            system.Run();

            var expected = new PositionConstraint(id: default, position: 1);
            Assert.That(component.Constraint, Is.EqualTo(expected));
        }
    }
}
