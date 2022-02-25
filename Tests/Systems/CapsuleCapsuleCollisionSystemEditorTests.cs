using andywiecko.BurstCollections;
using andywiecko.PBD2D.Core;
using andywiecko.PBD2D.Systems;
using NUnit.Framework;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace andywiecko.PBD2D.Editor.Tests
{
    public class CapsuleCapsuleCollisionSystemEditorTests
    {
        private class FakeComponent : FreeComponent, ICapsuleCollideWithCapsule
        {
            public float CollisionRadius { get; set; } = 1f;
            public Ref<NativeIndexedArray<Id<Point>, float2>> Positions { get; } = new NativeIndexedArray<Id<Point>, float2>(2, Allocator.Persistent);
            public Ref<NativeIndexedArray<Id<Point>, float2>> PredictedPositions { get; } = new NativeIndexedArray<Id<Point>, float2>(2, Allocator.Persistent);
            public Ref<NativeIndexedArray<Id<Point>, float>> MassesInv { get; } = new NativeIndexedArray<Id<Point>, float>(new[] { 1f, 1f }, Allocator.Persistent);
            public Ref<NativeIndexedArray<Id<Edge>, Edge>> Edges { get; } = new NativeIndexedArray<Id<Edge>, Edge>(new[] { (Edge)(0, 1) }, Allocator.Persistent);
            public Ref<NativeIndexedArray<Id<CollidableEdge>, Id<Edge>>> CollidableEdges { get; } = new NativeIndexedArray<Id<CollidableEdge>, Id<Edge>>(new[] { (Id<Edge>)0 }, Allocator.Persistent);
            public Ref<NativeIndexedArray<Id<CollidableEdge>, AABB>> AABBs { get; } = new NativeIndexedArray<Id<CollidableEdge>, AABB>(1, Allocator.Persistent);

            public float Friction => throw new System.NotImplementedException();

            public override void Dispose()
            {
                base.Dispose();
                Positions.Dispose();
                PredictedPositions.Dispose();
                MassesInv.Dispose();
                Edges.Dispose();
                CollidableEdges.Dispose();
                AABBs.Dispose();
            }

            public FakeComponent SetPositions(float2[] positions)
            {
                Positions.Value.GetInnerArray().CopyFrom(positions);
                return this;
            }

            public FakeComponent SetPredictedPositions(float2[] positions)
            {
                PredictedPositions.Value.GetInnerArray().CopyFrom(positions);
                return this;
            }
        }

        private class FakeTuple : FreeComponent, ICapsuleCapsuleCollisionTuple
        {
            public float Friction { get; set; } = 0;
            public Ref<NativeList<IdPair<Edge>>> PotentialCollisions { get; } = new NativeList<IdPair<Edge>>(64, Allocator.Persistent);
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

        private float2[] PredictedPositions1 => component1.PredictedPositions.Value.ToArray();
        private float2[] PredictedPositions2 => component2.PredictedPositions.Value.ToArray();

        private FakeComponent component1;
        private FakeComponent component2;
        private FakeTuple tuple;

        private CapsuleCapsuleCollisionSystem system;

        [SetUp]
        public void SetUp()
        {
            TestUtils.New(ref system);
            component1 = new();
            component2 = new();
            tuple = new() { Component1 = component1, Component2 = component2 };
        }

        [TearDown]
        public void TearDown()
        {
            tuple.Dispose();
            component2.Dispose();
            component1.Dispose();
        }

        [Test]
        public void PotentialCollisionBufferTest()
        {
            component1.SetPredictedPositions(new[] { math.float2(0, 0), math.float2(10, 0) });
            component2.SetPredictedPositions(new[] { math.float2(5, 1), math.float2(5f, 10f) });

            system.Schedule().Complete();

            IdPair<Edge> expectedPair = ((Id<Edge>)0, (Id<Edge>)0);
            Assert.That(tuple.PotentialCollisions.Value.ToArray(), Is.EqualTo(new[] { expectedPair }));
        }

        [Test]
        public void CollisionBufferTest()
        {
            component1.SetPredictedPositions(new[] { math.float2(0, 0), math.float2(10, 0) });
            component2.SetPredictedPositions(new[] { math.float2(5, 1), math.float2(5f, 10f) });

            system.Schedule().Complete();

            var expectedContact = new EdgeEdgeContactInfo(
                barPointA: 0.5f, barPointB: math.float2(1, 0),
                Id<Edge>.Zero, Id<Edge>.Zero
            );
            Assert.That(tuple.Collisions.Value.ToArray(), Is.EqualTo(new[] { expectedContact }));
        }

        [Test]
        public void OrthogonalCapsulesCollisionTest()
        {
            component1.SetPredictedPositions(new[] { math.float2(0, 0), math.float2(10, 0) });
            component2.SetPredictedPositions(new[] { math.float2(5, 1), math.float2(5f, 10f) });

            system.Schedule().Complete();

            var expectedContact = new EdgeEdgeContactInfo(
                barPointA: 0.5f, barPointB: math.float2(1, 0),
                edgeIdA: Id<Edge>.Zero, edgeIdB: Id<Edge>.Zero
            );
            Assert.That(tuple.Collisions.Value.ToArray(), Is.EqualTo(new[] { expectedContact }));
            float2[] expectedPositions1 = { math.float2(0, -1 / 3f), math.float2(10, -1 / 3f) };
            Assert.That(PredictedPositions1, Is.EqualTo(expectedPositions1));
            float2[] expectedPositions2 = { math.float2(5, 1 + 2 / 3f), math.float2(5, 10) };
            Assert.That(PredictedPositions2, Is.EqualTo(expectedPositions2));
        }

        [Test]
        public void ParallelCapsulesCollisionTest()
        {
            component1.SetPredictedPositions(new[] { math.float2(0, 0), math.float2(10, 0) });
            component2.SetPredictedPositions(new[] { math.float2(0, 1), math.float2(10, 1) });

            system.Schedule().Complete();
            // Should be run twice since method for finding closest point 
            // currenty returns arbitrary solution for parrallel lines
            // (since there are infinite correct solutions).
            system.Schedule().Complete();

            float2[] expectedPositions1 = { math.float2(0, -0.5f), math.float2(10, -0.5f) };
            Assert.That(PredictedPositions1, Is.EqualTo(expectedPositions1));
            float2[] expectedPositions2 = { math.float2(0, 1.5f), math.float2(10, 1.5f) };
            Assert.That(PredictedPositions2, Is.EqualTo(expectedPositions2));
        }

        [Test]
        public void FrictionCollisionTest()
        {
            tuple.Friction = 100;
            component1.SetPositions(new[] { math.float2(-1, 0), math.float2(9, 0) });
            component2.SetPositions(new[] { math.float2(5, 2), math.float2(5f, 10f) });
            component1.SetPredictedPositions(new[] { math.float2(0, 0), math.float2(10, 0) });
            component2.SetPredictedPositions(new[] { math.float2(5, 1), math.float2(5, 10) });

            system.Schedule().Complete();

            Debug.Log(component1.PredictedPositions.Value[(Id<Point>)0]);
            Debug.Log(component1.PredictedPositions.Value[(Id<Point>)1]);
            Debug.Log("");
            Debug.Log(component2.PredictedPositions.Value[(Id<Point>)0]);
            Debug.Log(component2.PredictedPositions.Value[(Id<Point>)1]);

            float2[] expectedPositions1 = { math.float2(0 - 1 / 3f, -1 / 3f), math.float2(10 - 1 / 3f, -1 / 3f) };
            Assert.That(PredictedPositions1, Is.EqualTo(expectedPositions1));
            float2[] expectedPositions2 = { math.float2(5 + 2 / 3f, 1 + 2 / 3f), math.float2(5, 10) };
            Assert.That(PredictedPositions2, Is.EqualTo(expectedPositions2));
        }
    }
}