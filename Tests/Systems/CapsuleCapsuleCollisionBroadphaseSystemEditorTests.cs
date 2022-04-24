using andywiecko.BurstCollections;
using andywiecko.PBD2D.Core;
using andywiecko.PBD2D.Systems;
using NUnit.Framework;
using System.Linq;
using Unity.Collections;

namespace andywiecko.PBD2D.Editor.Tests
{
    public class CapsuleCapsuleCollisionBroadphaseSystemEditorTests
    {
        private class FakeComponent : TestComponent, ICapsuleCollideWithCapsuleBroadphase
        {
            public Ref<NativeBoundingVolumeTree<AABB>> Tree { get; private set; }

            public void ConstructTree(AABB[] volumes)
            {
                Tree = new NativeBoundingVolumeTree<AABB>(volumes.Length, Allocator.Persistent);
                using var tmp = new NativeArray<AABB>(volumes, Allocator.Persistent);
                Tree.Value.Construct(tmp.AsReadOnly());
            }

            public override void Dispose()
            {
                Tree?.Dispose();
            }
        }

        private class FakeTuple : TestComponent, ICapsuleCapsuleCollisionBroadphaseTuple
        {
            public (int, int)[] Result => PotentialCollisions.Value.ToArray().Select(i => ((int)i.Id1, (int)i.Id2)).ToArray();

            public Ref<NativeList<IdPair<CollidableEdge>>> PotentialCollisions { get; } = new NativeList<IdPair<CollidableEdge>>(64, Allocator.Persistent);
            public FakeComponent Component1 { get; } = new();
            public FakeComponent Component2 { get; } = new();
            ICapsuleCollideWithCapsuleBroadphase ICapsuleCapsuleCollisionBroadphaseTuple.Component1 => Component1;
            ICapsuleCollideWithCapsuleBroadphase ICapsuleCapsuleCollisionBroadphaseTuple.Component2 => Component2;

            public override void Dispose()
            {
                PotentialCollisions?.Dispose();
                Component1?.Dispose();
                Component2?.Dispose();
            }
        }

        private CapsuleCapsuleCollisionBroadphaseSystem system;
        private FakeTuple tuple;

        [SetUp]
        public void SetUp()
        {
            TestUtils.New(ref system);
            tuple = new();
            system.World = new FakeWorld(tuple, tuple.Component1, tuple.Component2);
        }

        [TearDown]
        public void TearDown()
        {
            tuple?.Dispose();
        }

        private static readonly TestCaseData[] broadphaseResultTestData = new[]
        {
            new TestCaseData(
                new AABB[] { new(0, 1) },
                new AABB[] { new(0, 1) }
            ) { ExpectedResult = new []{ (0, 0) }, TestName = "Test case 1" },

            new TestCaseData(
                new AABB[] { new(0, 1) },
                new AABB[] { new(2, 3) }
            ) { ExpectedResult = new (int, int)[]{ }, TestName = "Test case 2" },

            new TestCaseData(
                new AABB[] { new(0, 1), new(1, 2), new(2.25f, 2.75f) },
                new AABB[] { new(2.25f, 2.75f), new(3, 4), new(4, 5) }
            ) { ExpectedResult = new []{ (2, 0) }, TestName = "Test case 3" },
        };

        [Test, TestCaseSource(nameof(broadphaseResultTestData))]
        public (int, int)[] BroadphaseResultTest(AABB[] volumes1, AABB[] volumes2)
        {
            tuple.Component1.ConstructTree(volumes1);
            tuple.Component2.ConstructTree(volumes2);
            system.Run();
            return tuple.Result;
        }
    }
}
