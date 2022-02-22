using andywiecko.PBD2D.Core;
using Unity.Collections;

namespace andywiecko.PBD2D.Components
{
    public class TriMeshCapsulesTriMeshCapsulesCollisionTuple : ComponentsTuple<
        ITriMeshCapsulesCollideWithTriMeshCapsules, ITriMeshCapsulesCollideWithTriMeshCapsules>,
        ICapsuleCapsuleCollisionTuple
    {
        public Ref<NativeList<EdgePair>> PotentialCollisions { get; private set; }
        public Ref<NativeList<EdgeEdgeContactInfo>> Collisions { get; private set; }

        public ICapsuleCollideWithCapsule Component1 => Item1;
        public ICapsuleCollideWithCapsule Component2 => Item2;

        public float Friction => 0.5f * (Component1.Friction + Component2.Friction);

        protected override void Initialize()
        {
            var allocator = Allocator.Persistent;

            DisposeOnDestroy(
                PotentialCollisions = new NativeList<EdgePair>(initialCapacity: 256, allocator),
                Collisions = new NativeList<EdgeEdgeContactInfo>(initialCapacity: 256, allocator)
            );
        }

        protected override bool InstantiateWhen(
            ITriMeshCapsulesCollideWithTriMeshCapsules c1,
            ITriMeshCapsulesCollideWithTriMeshCapsules c2)
            => c1.Id < c2.Id;
    }

    public class RodCapsulesTriMeshCapsulesCollisionTuple : ComponentsTuple<
        IRodCapsulesCollideWithTriMeshCapsules, ITriMeshCapsulesCollideWithRodCapsules>,
        ICapsuleCapsuleCollisionTuple
    {
        public Ref<NativeList<EdgePair>> PotentialCollisions { get; private set; }
        public Ref<NativeList<EdgeEdgeContactInfo>> Collisions { get; private set; }

        public ICapsuleCollideWithCapsule Component1 => Item1;
        public ICapsuleCollideWithCapsule Component2 => Item2;

        public float Friction => 0.5f * (Component1.Friction + Component2.Friction);

        protected override void Initialize()
        {
            var allocator = Allocator.Persistent;

            DisposeOnDestroy(
                PotentialCollisions = new NativeList<EdgePair>(initialCapacity: 256, allocator),
                Collisions = new NativeList<EdgeEdgeContactInfo>(initialCapacity: 256, allocator)
            );
        }

        protected override bool InstantiateWhen(
            IRodCapsulesCollideWithTriMeshCapsules c1,
            ITriMeshCapsulesCollideWithRodCapsules c2)
            => c1.Id < c2.Id;
    }
}