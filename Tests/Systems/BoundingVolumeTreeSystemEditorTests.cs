using andywiecko.BurstCollections;
using andywiecko.PBD2D.Core;
using andywiecko.PBD2D.Systems;
using NUnit.Framework;
using Unity.Collections;
using Unity.Mathematics;

namespace andywiecko.PBD2D.Editor.Tests
{
    public class BoundingVolumeTreeSystemEditorTests
    {
        private struct FakeStruct : IConvertableToAABB
        {
            public Id<Point> Id;
            public AABB ToAABB(NativeIndexedArray<Id<Point>, float2>.ReadOnly positions, float margin = 0) => new(positions[Id] - margin, positions[Id] + margin);
        }

        private class FakeComponent : TestComponent, IBoundingVolumeTreeComponent<FakeStruct>
        {
            private const int Count = 1;
            public AABB Bounds { get; set; }
            public float Margin { get; set; } = 0;
            public Ref<NativeBoundingVolumeTree<AABB>> Tree { get; } = new NativeBoundingVolumeTree<AABB>(Count, Allocator.Persistent);
            public Ref<NativeArray<AABB>> Volumes { get; } = new NativeArray<AABB>(Count, Allocator.Persistent);
            public Ref<NativeArray<FakeStruct>> Objects { get; } = new NativeArray<FakeStruct>(new FakeStruct[] { default }, Allocator.Persistent);
            public Ref<NativeIndexedArray<Id<Point>, float2>> Positions { get; } = new NativeIndexedArray<Id<Point>, float2>(Count, Allocator.Persistent);

            public ref float2 Position(int i) => ref Positions.Value.ElementAt((Id<Point>)i);
            public void ConstructTree() => Tree.Value.Construct(Volumes.Value.AsReadOnly());
            public override void Dispose()
            {
                Tree?.Dispose();
                Volumes?.Dispose();
                Objects?.Dispose();
                Positions?.Dispose();
            }
        }

        [FakeSystem]
        private class FakeSystem : BoundingVolumeTreeSystem<FakeStruct> { }

        private FakeComponent component;
        private FakeSystem system;

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
        public void UpdateVolumesTest()
        {
            component.ConstructTree();
            var m = component.Margin = 1;
            var p = component.Position(0) = 4;

            system.Run();

            var aabb = new AABB(p - m, p + m);
            Assert.That(component.Tree.Value.Volumes[0], Is.EqualTo(aabb));
            Assert.That(component.Volumes.Value[0], Is.EqualTo(aabb));
        }
    }
}