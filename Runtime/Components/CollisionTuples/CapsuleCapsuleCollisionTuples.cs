using andywiecko.BurstCollections;
using andywiecko.ECS;
using andywiecko.PBD2D.Core;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;

namespace andywiecko.PBD2D.Components
{
    public class TriMeshCapsulesTriMeshCapsulesCollisionTuple : ComponentsTuple<
        ITriMeshCapsulesCollideWithTriMeshCapsules, ITriMeshCapsulesCollideWithTriMeshCapsules>,
        ICapsuleCapsuleCollisionTuple, IBoundingVolumeTreesIntersectionTuple
    {
        public TriMeshCapsulesTriMeshCapsulesCollisionTuple(ITriMeshCapsulesCollideWithTriMeshCapsules item1, ITriMeshCapsulesCollideWithTriMeshCapsules item2, ComponentsRegistry componentsRegistry) : base(item1, item2, componentsRegistry)
        {

        }

        public Ref<NativeList<IdPair<CollidableEdge>>> PotentialCollisions { get; private set; }
        public Ref<NativeList<EdgeEdgeContactInfo>> Collisions { get; private set; }

        public ICapsuleCollideWithCapsule Component1 => Item1;
        public ICapsuleCollideWithCapsule Component2 => Item2;

        public float Friction => 0.5f * (Component1.Friction + Component2.Friction);

        Ref<NativeBoundingVolumeTree<AABB>> IBoundingVolumeTreesIntersectionTuple.Tree1 => Item1.Tree;
        Ref<NativeBoundingVolumeTree<AABB>> IBoundingVolumeTreesIntersectionTuple.Tree2 => Item2.Tree;
        Ref<NativeList<int2>> IBoundingVolumeTreesIntersectionTuple.Result => result;

        AABB IBoundingVolumeTreesIntersectionTuple.Bounds1 => Item1.Bounds;
        AABB IBoundingVolumeTreesIntersectionTuple.Bounds2 => Item2.Bounds;

        private Ref<NativeList<int2>> result;

        protected override void Initialize()
        {
            var allocator = Allocator.Persistent;

            DisposeOnDestroy(
                PotentialCollisions = new NativeList<IdPair<CollidableEdge>>(initialCapacity: 256, allocator),
                Collisions = new NativeList<EdgeEdgeContactInfo>(initialCapacity: 256, allocator)
            );

            result = UnsafeUtility.As<NativeList<IdPair<CollidableEdge>>, NativeList<int2>>(ref PotentialCollisions.Value);
        }

#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
        private static void CheckStructLayouts()
        {
            if (UnsafeUtility.SizeOf<IdPair<CollidableEdge>>() != UnsafeUtility.SizeOf<int2>())
            {
                UnityEngine.Debug.LogError(
                    $"[{nameof(TriMeshCapsulesTriMeshCapsulesCollisionTuple)}]: " +
                    $"{nameof(IdPair<CollidableEdge>)} has different layout than {nameof(int2)}. " +
                    "Buffer after reinterpretion will contain garbage data!"
                );
            }
        }
#endif

        protected override bool InstantiateWhen(ITriMeshCapsulesCollideWithTriMeshCapsules c1, ITriMeshCapsulesCollideWithTriMeshCapsules c2) => c1.ComponentId < c2.ComponentId;
    }
}