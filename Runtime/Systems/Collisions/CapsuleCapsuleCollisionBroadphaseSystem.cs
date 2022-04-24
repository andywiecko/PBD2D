using andywiecko.BurstCollections;
using andywiecko.PBD2D.Core;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace andywiecko.PBD2D.Systems
{
    [AddComponentMenu("PBD2D:Systems/Collisions/Capsule Capsule Collision System (Broadphase)")]
    public class CapsuleCapsuleCollisionBroadphaseSystem : BaseSystem<ICapsuleCapsuleCollisionTuple>
    {
        [BurstCompile]
        private struct CheckForPotentialCollisionsTree : IJob
        {
            private NativeBoundingVolumeTree<AABB>.ReadOnly tree1;
            private NativeBoundingVolumeTree<AABB>.ReadOnly tree2;
            private NativeList<IdPair<CollidableEdge>> potentialCollisions;

            public CheckForPotentialCollisionsTree(ICapsuleCapsuleCollisionTuple tuple)
            {
                tree1 = tuple.Component1.Tree.Value.AsReadOnly();
                tree2 = tuple.Component2.Tree.Value.AsReadOnly();
                potentialCollisions = tuple.PotentialCollisions;
            }

            public void Execute()
            {
                // TODO: remove temporary allocation
                using var result = new NativeList<int2>(Allocator.Temp);
                tree1.GetIntersectionsWithTree(tree2, result);

                // TODO Reinterpret?
                foreach (var p in result)
                {
                    var i = (Id<CollidableEdge>)p.x;
                    var j = (Id<CollidableEdge>)p.y;
                    potentialCollisions.Add(new(i, j));
                }
            }
        }

        public override JobHandle Schedule(JobHandle dependencies = default)
        {
            foreach (var component in References)
            {
                dependencies = new CommonJobs.ClearListJob<IdPair<CollidableEdge>>(component.PotentialCollisions.Value).Schedule(dependencies);
                dependencies = new CheckForPotentialCollisionsTree(component).Schedule(dependencies);
            }

            return dependencies;
        }
    }
}