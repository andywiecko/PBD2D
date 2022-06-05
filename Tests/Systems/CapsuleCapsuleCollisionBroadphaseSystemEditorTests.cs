using andywiecko.BurstCollections;
using andywiecko.PBD2D.Core;
using andywiecko.PBD2D.Systems;
using NUnit.Framework;
using System.Linq;
using Unity.Collections;
using Unity.Mathematics;

namespace andywiecko.PBD2D.Editor.Tests
{
    public class CapsuleCapsuleCollisionBroadphaseSystemEditorTests
    {
        private class FakeComponent : TestComponent
        {
            public Ref<NativeBoundingVolumeTree<AABB>> Tree { get; private set; }

            public void ConstructTree(AABB[] volumes)
            {
                Tree = new NativeBoundingVolumeTree<AABB>(volumes.Length, Allocator.Persistent);
                using var tmp = new NativeArray<AABB>(volumes, Allocator.Persistent);
                Tree.Value.Construct(tmp.AsReadOnly());
            }

            public override void Dispose() => Tree?.Dispose();
        }

        private class FakeTuple : TestComponent, IBoundingVolumeTreesIntersectionTuple
        {
            public AABB Bounds1 { get; }
            public AABB Bounds2 { get; }
            public Ref<NativeBoundingVolumeTree<AABB>> Tree1 { get; private set; }
            public Ref<NativeBoundingVolumeTree<AABB>> Tree2 { get; private set; }
            public Ref<NativeList<int2>> Result { get; } = new NativeList<int2>(64, Allocator.Persistent);

            public (int, int)[] GetResult() => Result.Value.ToArray().Select(i => (i.x, i.y)).ToArray();
            public void ConstructTree1(AABB[] volumes)
            {
                Tree1 = new NativeBoundingVolumeTree<AABB>(volumes.Length, Allocator.Persistent);
                using var tmp = new NativeArray<AABB>(volumes, Allocator.Persistent);
                Tree1.Value.Construct(tmp.AsReadOnly());
            }

            public void ConstructTree2(AABB[] volumes)
            {
                Tree2 = new NativeBoundingVolumeTree<AABB>(volumes.Length, Allocator.Persistent);
                using var tmp = new NativeArray<AABB>(volumes, Allocator.Persistent);
                Tree2.Value.Construct(tmp.AsReadOnly());
            }

            public override void Dispose()
            {
                Result?.Dispose();
                Tree1?.Dispose();
                Tree2?.Dispose();
            }
        }

        private BoundingVolumeTreesIntersectionSystem system;
        private FakeTuple tuple;

        [SetUp]
        public void SetUp()
        {
            tuple = new();
            system = new() { World = new FakeWorld(tuple) };
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
            tuple.ConstructTree1(volumes1);
            tuple.ConstructTree2(volumes2);
            system.Run();
            return tuple.GetResult();
        }
    }
}
