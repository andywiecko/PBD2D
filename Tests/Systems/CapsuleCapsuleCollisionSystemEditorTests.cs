using andywiecko.BurstCollections;
using andywiecko.PBD2D.Core;
using andywiecko.PBD2D.Systems;
using NUnit.Framework;
using Unity.Collections;
using Unity.Mathematics;

namespace andywiecko.PBD2D.Editor.Tests
{
    public class CapsuleCapsuleCollisionSystemEditorTests
    {
        private class FakeComponent : TestComponent, ICapsuleCollideWithCapsule
        {
            public AABB Bounds { get; }
            public float CollisionRadius { get; set; } = 1f;
            public Ref<NativeIndexedArray<Id<Point>, float2>> PreviousPositions { get; } = new NativeIndexedArray<Id<Point>, float2>(2, Allocator.Persistent);
            public Ref<NativeIndexedArray<Id<Point>, float2>> Positions { get; } = new NativeIndexedArray<Id<Point>, float2>(2, Allocator.Persistent);
            public Ref<NativeIndexedArray<Id<Point>, float>> Weights { get; } = new NativeIndexedArray<Id<Point>, float>(new[] { 1f, 1f }, Allocator.Persistent);
            public Ref<NativeIndexedArray<Id<CollidableEdge>, CollidableEdge>> CollidableEdges { get; } = new NativeIndexedArray<Id<CollidableEdge>, CollidableEdge>(new CollidableEdge[] { (0, 1) }, Allocator.Persistent);
            public float Friction => throw new System.NotImplementedException(); // Used friction from tuple explicitly

            public override void Dispose()
            {
                base.Dispose();
                PreviousPositions?.Dispose();
                Positions?.Dispose();
                Weights?.Dispose();
                CollidableEdges?.Dispose();
            }

            public FakeComponent SetPreviousPositions(float2[] positions)
            {
                PreviousPositions.Value.GetInnerArray().CopyFrom(positions);
                return this;
            }

            public FakeComponent SetPositions(float2[] positions)
            {
                Positions.Value.GetInnerArray().CopyFrom(positions);
                return this;
            }
        }

        private class FakeTuple : TestComponent, ICapsuleCapsuleCollisionTuple
        {
            public float Friction { get; set; } = 0;
            public Ref<NativeList<IdPair<CollidableEdge>>> PotentialCollisions { get; } = new NativeList<IdPair<CollidableEdge>>(64, Allocator.Persistent);
            public Ref<NativeList<EdgeEdgeContactInfo>> Collisions { get; } = new NativeList<EdgeEdgeContactInfo>(64, Allocator.Persistent);

            public ICapsuleCollideWithCapsule Component1 { get; set; }
            public ICapsuleCollideWithCapsule Component2 { get; set; }

            public override void Dispose()
            {
                base.Dispose();
                PotentialCollisions?.Dispose();
                Collisions?.Dispose();
            }
        }

        private float2[] Positions1 => component1.Positions.Value.ToArray();
        private float2[] Positions2 => component2.Positions.Value.ToArray();

        private FakeComponent component1;
        private FakeComponent component2;
        private FakeTuple tuple;

        private CapsuleCapsuleCollisionSystem system;

        [SetUp]
        public void SetUp()
        {
            component1 = new();
            component2 = new();
            tuple = new() { Component1 = component1, Component2 = component2 };
            system = new() { World = new FakeWorld(component1, component2, tuple) };
        }

        [TearDown]
        public void TearDown()
        {
            tuple.Dispose();
            component2.Dispose();
            component1.Dispose();
        }

        [Test]
        public void CollisionBufferTest()
        {
            component1.SetPositions(new[] { math.float2(0, 0), math.float2(10, 0) });
            component2.SetPositions(new[] { math.float2(5, 1), math.float2(5f, 10f) });
            tuple.PotentialCollisions.Value.Add(new(Id<CollidableEdge>.Zero, Id<CollidableEdge>.Zero));

            system.Run();

            var expectedContact = new EdgeEdgeContactInfo(
                barPointA: 0.5f, barPointB: math.float2(1, 0),
                Id<CollidableEdge>.Zero, Id<CollidableEdge>.Zero
            );
            Assert.That(tuple.Collisions.Value.ToArray(), Is.EqualTo(new[] { expectedContact }));
        }

        [Test]
        public void OrthogonalCapsulesCollisionTest()
        {
            component1.SetPositions(new[] { math.float2(0, 0), math.float2(10, 0) });
            component2.SetPositions(new[] { math.float2(5, 1), math.float2(5f, 10f) });
            tuple.PotentialCollisions.Value.Add(new(Id<CollidableEdge>.Zero, Id<CollidableEdge>.Zero));

            system.Run();

            var expectedContact = new EdgeEdgeContactInfo(
                barPointA: 0.5f, barPointB: math.float2(1, 0),
                edgeIdA: Id<CollidableEdge>.Zero, edgeIdB: Id<CollidableEdge>.Zero
            );
            Assert.That(tuple.Collisions.Value.ToArray(), Is.EqualTo(new[] { expectedContact }));
            float2[] expectedPositions1 = { math.float2(0, -1 / 3f), math.float2(10, -1 / 3f) };
            Assert.That(Positions1, Is.EqualTo(expectedPositions1));
            float2[] expectedPositions2 = { math.float2(5, 1 + 2 / 3f), math.float2(5, 10) };
            Assert.That(Positions2, Is.EqualTo(expectedPositions2));
        }

        [Test]
        public void ParallelCapsulesCollisionTest()
        {
            component1.SetPositions(new[] { math.float2(0, 0), math.float2(10, 0) });
            component2.SetPositions(new[] { math.float2(0, 1), math.float2(10, 1) });
            tuple.PotentialCollisions.Value.Add(new(Id<CollidableEdge>.Zero, Id<CollidableEdge>.Zero));

            system.Run();
            // Should be run twice since method for finding closest point 
            // currently returns arbitrary solution for parallel lines
            // (since there are infinite correct solutions).
            system.Run();

            float2[] expectedPositions1 = { math.float2(0, -0.5f), math.float2(10, -0.5f) };
            Assert.That(Positions1, Is.EqualTo(expectedPositions1));
            float2[] expectedPositions2 = { math.float2(0, 1.5f), math.float2(10, 1.5f) };
            Assert.That(Positions2, Is.EqualTo(expectedPositions2));
        }

        [Test]
        public void FrictionCollisionTest()
        {
            tuple.Friction = 100;
            component1.SetPreviousPositions(new[] { math.float2(-1, 0), math.float2(9, 0) });
            component2.SetPreviousPositions(new[] { math.float2(5, 2), math.float2(5f, 10f) });
            component1.SetPositions(new[] { math.float2(0, 0), math.float2(10, 0) });
            component2.SetPositions(new[] { math.float2(5, 1), math.float2(5, 10) });
            tuple.PotentialCollisions.Value.Add(new(Id<CollidableEdge>.Zero, Id<CollidableEdge>.Zero));

            system.Run();

            float2[] expectedPositions1 = { math.float2(0 - 1 / 3f, -1 / 3f), math.float2(10 - 1 / 3f, -1 / 3f) };
            Assert.That(Positions1, Is.EqualTo(expectedPositions1));
            float2[] expectedPositions2 = { math.float2(5 + 2 / 3f, 1 + 2 / 3f), math.float2(5, 10) };
            Assert.That(Positions2, Is.EqualTo(expectedPositions2));
        }
    }
}