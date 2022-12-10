using andywiecko.BurstCollections;
using andywiecko.BurstMathUtils;
using andywiecko.PBD2D.Core;
using andywiecko.PBD2D.Systems;
using NUnit.Framework;
using Unity.Collections;
using Unity.Mathematics;

namespace andywiecko.PBD2D.Editor.Tests
{
    public class StencilBendingConstraintsSystemEditorTests
    {
        private class FakeComponent : TestComponent, IStencilBendingConstraints
        {
            private static readonly Stencil Stencil = new(new(0), new(1), new(2));

            public float RestAngle => constraints.Value[default].RestAngle;
            public float Stiffness { get; set; } = 1;
            public float Compliance { get; set; } = 0;

            public float2[] Positions { get => positions.Value.ToArray(); set => positions.Value.GetInnerArray().CopyFrom(value); }
            public float[] Weights { get => weights.Value.ToArray(); set => weights.Value.GetInnerArray().CopyFrom(value); }

            Ref<NativeIndexedArray<Id<Point>, float2>> IStencilBendingConstraints.Positions => positions;
            Ref<NativeIndexedArray<Id<Point>, float>> IStencilBendingConstraints.Weights => weights;
            Ref<NativeList<StencilBendingConstraint>> IStencilBendingConstraints.Constraints => constraints;

            private readonly Ref<NativeIndexedArray<Id<Point>, float2>> positions = new(new(3, Allocator.Persistent));
            private readonly Ref<NativeIndexedArray<Id<Point>, float>> weights = new(new(3, Allocator.Persistent)
            {
                [(Id<Point>)0] = 1f,
                [(Id<Point>)1] = 1f,
                [(Id<Point>)2] = 1f
            });
            private readonly Ref<NativeList<StencilBendingConstraint>> constraints = new(new(Allocator.Persistent)
            {
                Length = 1,
            });

            public void GenerateConstraints()
            {
                constraints.Value[default] = StencilBendingConstraint.Create(Stencil, positions.Value);
            }

            public override void Dispose()
            {
                positions?.Dispose();
                weights?.Dispose();
                constraints?.Dispose();
            }

            public float Angle()
            {
                var p = Positions;
                return MathUtils.Angle(p[1] - p[0], p[2] - p[1]);
            }
        }

        private StencilBendingConstraintsSystem system;
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

        [Test]
        public void IdentityTest()
        {
            var initialPositions = component.Positions = new float2[] { new(0, 0), new(1, 0), new(1, 1) };
            component.Weights = new float[] { 1, 1, 1 };
            component.GenerateConstraints();

            system.Run();

            Assert.That(component.Positions, Is.EqualTo(initialPositions));
        }

        public static readonly TestCaseData[] bendingTestData =
        {
            new(new float2[]{ new(0, 0), new(1, 0), new(1, 1) })
            {
                TestName = "Test case 1 (right angle)",
            },
            new(new float2[]{ new(0, 0), new(1, 0), new(0, 1) })
            {
                TestName = "Test case 2 (accute angle)",
            },
            new(new float2[]{ new(0, 0), new(1, 0), new(0, -1) })
            {
                TestName = "Test case 3 (obtuse angle)",
            },
            new(new float2[]{ new(0, 0), new(1, 0), new(0, 0) })
            {
                TestName = "Test case 4 (zero angle)",
            },
        };

        [Test, TestCaseSource(nameof(bendingTestData))]
        public void BendingTest(float2[] positions)
        {
            component.Positions = positions;
            component.Weights = new float[] { 1, 1, 1 };
            component.GenerateConstraints();
            component.Positions = new float2[] { new(0, 0), new(1, 0), new(2, 0) };

            for (int i = 0; i < 5; i++)
            {
                system.Run();
            }

            Assert.That(component.Angle, Is.EqualTo(component.RestAngle).Within(0.001f));
        }

        [Test]
        public void WeightsTest()
        {
            var initialPositions = component.Positions = new float2[] { new(0, 0), new(1, 0), new(1, 1) };
            component.Weights = new float[] { 0, 0, 1 };
            component.GenerateConstraints();
            component.Positions = new float2[] { new(0, 0), new(1, 0), new(2, 0) };

            for (int i = 0; i < 5; i++)
            {
                system.Run();
            }

            Assert.That(component.Positions[..2], Is.EqualTo(initialPositions[..2]));
            Assert.That(component.Angle, Is.EqualTo(component.RestAngle).Within(0.001f));
        }

        [Test]
        public void StiffnessTest()
        {
            void Setup(float stiffness)
            {
                component.Positions = new float2[] { new(0, 0), new(1, 0), new(1, 1) };
                component.Weights = new float[] { 0, 0, 1 };
                component.GenerateConstraints();
                component.Positions = new float2[] { new(0, 0), new(1, 0), new(2, 0) };
                component.Stiffness = stiffness;
            }

            float Measure() => math.abs(component.Angle() - component.RestAngle);

            Setup(stiffness: 1);
            system.Run();
            var angle0 = Measure();

            Setup(stiffness: 0.1f);
            system.Run();
            var angle1 = Measure();

            Setup(stiffness: 0.01f);
            system.Run();
            var angle2 = Measure();

            Assert.That(angle0, Is.LessThan(angle1));
            Assert.That(angle1, Is.LessThan(angle2));
        }

        [Test]
        public void ComplianceTest()
        {
            void Setup(float compliance)
            {
                component.Positions = new float2[] { new(0, 0), new(1, 0), new(1, 1) };
                component.Weights = new float[] { 0, 0, 1 };
                component.GenerateConstraints();
                component.Positions = new float2[] { new(0, 0), new(1, 0), new(2, 0) };
                component.Compliance = compliance;
            }

            float Measure() => math.abs(component.Angle() - component.RestAngle);

            Setup(compliance: 0);
            system.Run();
            var angle0 = Measure();

            Setup(compliance: 0.01f);
            system.Run();
            var angle1 = Measure();

            Setup(compliance: 0.1f);
            system.Run();
            var angle2 = Measure();

            Assert.That(angle0, Is.LessThan(angle1));
            Assert.That(angle1, Is.LessThan(angle2));
        }
    }
}