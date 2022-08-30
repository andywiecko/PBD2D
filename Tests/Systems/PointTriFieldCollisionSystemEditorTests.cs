using andywiecko.BurstCollections;
using andywiecko.ECS;
using andywiecko.PBD2D.Core;
using andywiecko.PBD2D.Systems;
using NUnit.Framework;
using System;
using Unity.Collections;
using Unity.Mathematics;

namespace andywiecko.PBD2D.Editor.Tests
{
    public class PointTriFieldCollisionSystemEditorTests
    {
        private struct FakeLookup : ITriFieldLookup
        {
            public Id<ExternalEdge> Id { get; set; }
            public Id<ExternalEdge> GetExternalEdge(Id<Triangle> triId, float3 bar) => Id;
        }

        private class FakeTuple : TestComponent, IPointTriFieldCollisionTuple<FakeLookup>
        {
            public float Friction { get; set; } = 0;
            public Ref<NativeList<IdPair<Point, Triangle>>> PotentialCollisions { get; } = new(new(64, Allocator.Persistent));
            public Ref<NativeList<IdPair<Point, ExternalEdge>>> Collisions { get; } = new(new(64, Allocator.Persistent));
            public IPointCollideWithTriField PointsComponent { get; set; }
            public ITriFieldCollideWithPoint<FakeLookup> TriFieldComponent { get; set; }
            public IdPair<Point, ExternalEdge>[] Result => Collisions.Value.ToArray();
            public override void Dispose()
            {
                PotentialCollisions?.Dispose();
                Collisions?.Dispose();
            }
            public void AddPotentialCollision((int i, int j) tuple) => PotentialCollisions.Value.Add(new((Id<Point>)tuple.i, (Id<Triangle>)tuple.j));
        }

        private class FakePointsComponent : TestComponent, IPointCollideWithTriField
        {
            float IPointCollideWithTriField.Friction => throw new NotImplementedException();
            public AABB Bounds => default;
            public Id<IEntity> EntityId => default;
            public Ref<NativeIndexedArray<Id<Point>, float2>> Positions { get; } = new(new(1, Allocator.Persistent));
            public Ref<NativeIndexedArray<Id<Point>, float2>> PreviousPositions { get; } = new(new(1, Allocator.Persistent));
            public Ref<NativeIndexedArray<Id<Point>, float>> Weights { get; } = new(new(1, Allocator.Persistent) { [default] = 1 });
            public ref float2 Position => ref Positions.Value.ElementAt(default);
            public ref float2 PreviousPosition => ref PreviousPositions.Value.ElementAt(default);
            public ref float Weight => ref Weights.Value.ElementAt(default);
            public override void Dispose()
            {
                Positions?.Dispose();
                PreviousPositions?.Dispose();
                Weights?.Dispose();
            }
        }

        private class FakeTriFieldComponent : TestComponent, ITriFieldCollideWithPoint<FakeLookup>
        {
            private const int PointsCount = 3 * TrianglesCount;
            private const int TrianglesCount = 1;

            float ITriFieldCollideWithPoint<FakeLookup>.Friction => throw new NotImplementedException();
            public AABB Bounds => default;
            public Id<IEntity> EntityId => default;
            public Ref<NativeIndexedArray<Id<Point>, float2>> Positions { get; } = new(new(PointsCount, Allocator.Persistent));
            public Ref<NativeIndexedArray<Id<Point>, float2>> PreviousPositions { get; } = new(new(PointsCount, Allocator.Persistent));
            public Ref<NativeIndexedArray<Id<Point>, float>> Weights { get; } = new(new(PointsCount, Allocator.Persistent) { [(Id<Point>)0] = 1, [(Id<Point>)1] = 1, [(Id<Point>)2] = 1 });
            public Ref<NativeIndexedArray<Id<Triangle>, Triangle>> Triangles { get; } = new(new(TrianglesCount, Allocator.Persistent) { [default] = (0, 1, 2) });
            public Ref<NativeIndexedArray<Id<ExternalEdge>, ExternalEdge>> ExternalEdges { get; } = new(new(new ExternalEdge[] { (1, 2), (0, 2), (1, 0) }, Allocator.Persistent));
            public ref float2 TrianglePosition(int i) => ref Positions.Value.ElementAt((Id<Point>)i);
            public ref float TriangleWeight(int i) => ref Weights.Value.ElementAt((Id<Point>)i);
            public ref float2 TrianglePreviousPosition(int i) => ref PreviousPositions.Value.ElementAt((Id<Point>)i);
            public FakeLookup TriFieldLookup { get; set; }
            public override void Dispose()
            {
                Positions?.Dispose();
                PreviousPositions?.Dispose();
                Weights?.Dispose();
                Triangles?.Dispose();
                ExternalEdges?.Dispose();
            }
        }

        private class FakeSystem : PointTriFieldCollisionSystem<FakeLookup> { }

        private FakePointsComponent points;
        private FakeTriFieldComponent triField;
        private FakeTuple tuple;
        private FakeSystem system;

        [SetUp]
        public void SetUp()
        {
            points = new();
            triField = new();
            tuple = new() { PointsComponent = points, TriFieldComponent = triField };
            system = new() { World = new FakeWorld(points, triField, tuple) };
        }

        [TearDown]
        public void TearDown()
        {
            points?.Dispose();
            triField?.Dispose();
            tuple?.Dispose();
        }

        [Test]
        public void IntersectionTest()
        {
            Assert.That(tuple.Intersecting(), Is.True);
        }

        [Test]
        public void DetectCollisionTest()
        {
            points.Position = new(0, 0.75f);
            points.Weight = 1;

            triField.TrianglePosition(0) = new(-1, 0);
            triField.TrianglePosition(1) = new(1, 0);
            triField.TrianglePosition(2) = new(0, 1);
            var id = (Id<ExternalEdge>)2;
            triField.TriFieldLookup = new() { Id = id };

            tuple.AddPotentialCollision(default);

            system.Run();

            var expectedResult = new IdPair<Point, ExternalEdge>(default, id);
            Assert.That(tuple.Result, Is.EqualTo(new[] { expectedResult }));
        }

        [Test]
        public void DetectCollisionTestWithoutPotentialCollision()
        {
            points.Position = new(0, 0.75f);
            points.Weight = 1;

            triField.TrianglePosition(0) = new(-1, 0);
            triField.TrianglePosition(1) = new(1, 0);
            triField.TrianglePosition(2) = new(0, 1);
            var id = (Id<ExternalEdge>)2;
            triField.TriFieldLookup = new() { Id = id };

            system.Run();

            Assert.That(tuple.Result, Is.Empty);
        }

        [Test]
        public void ClearCollisionTest()
        {
            tuple.Collisions.Value.Add(default);

            system.Run();

            Assert.That(tuple.Result, Is.Empty);
        }

        [Test]
        public void ResolveCollisionTest()
        {
            points.Position = new(0, 0.75f);
            points.Weight = 1;

            triField.TrianglePosition(0) = new(-1, 0);
            triField.TrianglePosition(1) = new(1, 0);
            triField.TrianglePosition(2) = new(0, 1);
            triField.TriFieldLookup = new() { Id = (Id<ExternalEdge>)2 };

            tuple.PotentialCollisions.Value.Add(default);

            system.Run();

            Assert.That(points.Position, Is.EqualTo(math.float2(0, 0.25f)));
            Assert.That(triField.TrianglePosition(0), Is.EqualTo(math.float2(-1, 0.25f)));
            Assert.That(triField.TrianglePosition(1), Is.EqualTo(math.float2(+1, 0.25f)));
            Assert.That(triField.TrianglePosition(2), Is.EqualTo(math.float2(0, 1)));
        }

        [Test]
        public void ResolveCollisionTestWithNonUniformMass()
        {
            points.Position = new(0, 0.75f);
            points.Weight = 0;

            triField.TrianglePosition(0) = new(-1, 0);
            triField.TrianglePosition(1) = new(1, 0);
            triField.TrianglePosition(2) = new(0, 1);
            triField.TriFieldLookup = new() { Id = (Id<ExternalEdge>)2 };

            tuple.PotentialCollisions.Value.Add(default);

            system.Run();

            Assert.That(points.Position, Is.EqualTo(math.float2(0, 0.75f)));
            Assert.That(triField.TrianglePosition(0), Is.EqualTo(math.float2(-1, 0.75f)));
            Assert.That(triField.TrianglePosition(1), Is.EqualTo(math.float2(+1, 0.75f)));
            Assert.That(triField.TrianglePosition(2), Is.EqualTo(math.float2(0, 1)));
        }

        [Test]
        public void ResolveCollisionTestWithResolvedCase()
        {
            var p = points.Position = new(0, -0.75f);
            points.Weight = 0;

            var q0 = triField.TrianglePosition(0) = new(-1, 0);
            var q1 = triField.TrianglePosition(1) = new(1, 0);
            var q2 = triField.TrianglePosition(2) = new(0, 1);
            triField.TriFieldLookup = new() { Id = (Id<ExternalEdge>)2 };

            tuple.PotentialCollisions.Value.Add(default);

            system.Run();

            Assert.That(points.Position, Is.EqualTo(p));
            Assert.That(triField.TrianglePosition(0), Is.EqualTo(q0));
            Assert.That(triField.TrianglePosition(1), Is.EqualTo(q1));
            Assert.That(triField.TrianglePosition(2), Is.EqualTo(q2));
        }

        [Test]
        public void FrictionTestWithStationaryEdge()
        {
            var p0 = points.PreviousPosition = new(-0.5f, 0);
            points.Position = new(0, 0.75f);
            points.Weight = 1;

            triField.TriangleWeight(0) = triField.TriangleWeight(1) = triField.TriangleWeight(2) = 0;
            var q0 = triField.TrianglePreviousPosition(0) = triField.TrianglePosition(0) = new(-1, 0);
            var q1 = triField.TrianglePreviousPosition(1) = triField.TrianglePosition(1) = new(1, 0);
            var q2 = triField.TrianglePreviousPosition(2) = triField.TrianglePosition(2) = new(0, 1);
            triField.TriFieldLookup = new() { Id = (Id<ExternalEdge>)2 };

            tuple.PotentialCollisions.Value.Add(default);
            tuple.Friction = 1;

            system.Run();

            Assert.That(points.Position, Is.EqualTo(p0));
            Assert.That(triField.TrianglePosition(0), Is.EqualTo(q0));
            Assert.That(triField.TrianglePosition(1), Is.EqualTo(q1));
            Assert.That(triField.TrianglePosition(2), Is.EqualTo(q2));
        }

        [Test]
        public void FrictionTestWithStationaryPoint()
        {
            var p = points.PreviousPosition = points.Position = new(0, 0.75f);
            points.Weight = 0;

            triField.TriangleWeight(0) = triField.TriangleWeight(1) = triField.TriangleWeight(2) = 1;
            var q0 = triField.TrianglePreviousPosition(0) = new(-1.5f, 0);
            var q1 = triField.TrianglePreviousPosition(1) = new(+0.5f, 0);
            var q2 = triField.TrianglePreviousPosition(2) = new(0, 1);
            triField.TrianglePosition(0) = new(-1, 0);
            triField.TrianglePosition(1) = new(1, 0);
            triField.TrianglePosition(2) = q2;
            triField.TriFieldLookup = new() { Id = (Id<ExternalEdge>)2 };

            tuple.PotentialCollisions.Value.Add(default);
            tuple.Friction = 1;

            system.Run();

            Assert.That(points.Position, Is.EqualTo(p));
            var dx = math.float2(0, 0.75f);
            Assert.That(triField.TrianglePosition(0), Is.EqualTo(q0 + dx));
            Assert.That(triField.TrianglePosition(1), Is.EqualTo(q1 + dx));
            Assert.That(triField.TrianglePosition(2), Is.EqualTo(q2));
        }
    }
}
