using andywiecko.BurstCollections;
using andywiecko.PBD2D.Core;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace andywiecko.PBD2D.Systems
{
    [AddComponentMenu("PBD2D:Systems/Collisions/Capsule Capsule Collision System (Broadphase)")]
    public class CapsuleCapsuleCollisionBroadphaseSystem : BaseSystem<ICapsuleCapsuleCollisionTuple>
    {
        // TODOs:
        // - Schedule for smaller tree (with smaller leaf count)
        // - Can be parallized or optimized, learn how to intersect two trees
        [BurstCompile]
        private struct CheckForPotentialCollisionsTree : IJob
        {
            private BoundingVolumeTree<AABB> tree1;
            private BoundingVolumeTree<AABB> tree2;
            private NativeList<IdPair<Edge>> potentialCollisions;
            private NativeIndexedArray<Id<CollidableEdge>, Id<Edge>>.ReadOnly collidableEdge1;
            private NativeIndexedArray<Id<CollidableEdge>, Id<Edge>>.ReadOnly collidableEdge2;

            public CheckForPotentialCollisionsTree(ICapsuleCapsuleCollisionTuple tuple)
            {
                tree1 = tuple.Component1.Tree;
                tree2 = tuple.Component2.Tree;
                potentialCollisions = tuple.PotentialCollisions;
                collidableEdge1 = tuple.Component1.CollidableEdges.Value.AsReadOnly();
                collidableEdge2 = tuple.Component2.CollidableEdges.Value.AsReadOnly();
            }

            public void Execute()
            {
                foreach(var i in 0..tree1.LeavesCount)
                {
                    Execute(i);
                }
            }

            public void Execute(int i)
            {
                var aabb1 = tree1.Volumes[i];
                var edgeId1 = collidableEdge1[(Id<CollidableEdge>)i];
                var bfs = tree2.BreadthFirstSearch;
                foreach (var (j, aabb2) in bfs)
                {
                    if (aabb1.Intersects(aabb2))
                    {
                        if (bfs.IsLeaf(j))
                        {
                            var edgeId2 = collidableEdge2[(Id<CollidableEdge>)j];
                            potentialCollisions.AddNoResize((edgeId1, edgeId2));
                        }
                        bfs.Traverse(j);
                    }
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