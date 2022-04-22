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
            private NativeList<IdPair<Edge>> potentialCollisions;
            private NativeIndexedArray<Id<CollidableEdge>, Id<Edge>>.ReadOnly collidableEdge1;
            private NativeIndexedArray<Id<CollidableEdge>, Id<Edge>>.ReadOnly collidableEdge2;

            public CheckForPotentialCollisionsTree(ICapsuleCapsuleCollisionTuple tuple)
            {
                tree1 = tuple.Component1.Tree.Value.AsReadOnly();
                tree2 = tuple.Component2.Tree.Value.AsReadOnly();
                potentialCollisions = tuple.PotentialCollisions;
                collidableEdge1 = tuple.Component1.CollidableEdges.Value.AsReadOnly();
                collidableEdge2 = tuple.Component2.CollidableEdges.Value.AsReadOnly();
            }

            public void Execute()
            {
                // TODO: remove temporary allocation
                using var result = new NativeList<int2>(Allocator.Temp);
                tree1.GetIntersectionsWithTree(tree2, result);
                foreach (var p in result)
                {
                    var i = collidableEdge1[(Id<CollidableEdge>)p.x];
                    var j = collidableEdge2[(Id<CollidableEdge>)p.y];
                    potentialCollisions.Add(new(i, j));
                }
            }
        }

        public override JobHandle Schedule(JobHandle dependencies = default)
        {
            foreach (var component in References)
            {
                dependencies = new CommonJobs.ClearListJob<IdPair<Edge>>(component.PotentialCollisions.Value).Schedule(dependencies);
                dependencies = new CheckForPotentialCollisionsTree(component).Schedule(dependencies);
            }

            return dependencies;
        }
    }
}