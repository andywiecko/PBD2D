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
        // - schedule for smaller tree
        // - Can be parallized or optimized, learn how to intersects to trees
        [BurstCompile]
        private struct CheckForPotentialCollisionsTree : IJob // 
        {
            //[ReadOnly]
            private BoundingVolumeTree<AABB> tree1;
            //[ReadOnly]
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
                var aabb = tree1.Volumes[i];
                var bfs = tree2.BreadthFirstSearch;
                foreach (var (id, nodeAABB) in bfs)
                {
                    if (aabb.Intersects(nodeAABB))
                    {
                        if (bfs.IsLeaf(id))
                        {
                            potentialCollisions.AddNoResize((collidableEdge1[(Id<CollidableEdge>)i], collidableEdge2[(Id<CollidableEdge>)id]));
                            //potentialCollisions.AddNoResize(((Id<Edge>)i, (Id<Edge>)id));
                        }
                        bfs.Traverse(id);
                    }
                }
            }
        }

        [BurstCompile]
        private struct CheckForPotentialCollisions : IJob
        {
            private NativeIndexedArray<Id<CollidableEdge>, AABB>.ReadOnly aabbs1;
            private NativeIndexedArray<Id<CollidableEdge>, AABB>.ReadOnly aabbs2;
            private NativeIndexedArray<Id<CollidableEdge>, Id<Edge>>.ReadOnly collidableEdges1;
            private NativeIndexedArray<Id<CollidableEdge>, Id<Edge>>.ReadOnly collidableEdges2;
            private NativeList<IdPair<Edge>> potentialCollisions;

            public CheckForPotentialCollisions(ICapsuleCapsuleCollisionTuple tuple)
            {
                var (c1, c2) = tuple;
                aabbs1 = c1.AABBs.Value.AsReadOnly();
                aabbs2 = c2.AABBs.Value.AsReadOnly();
                collidableEdges1 = c1.CollidableEdges.Value.AsReadOnly();
                collidableEdges2 = c2.CollidableEdges.Value.AsReadOnly();

                potentialCollisions = tuple.PotentialCollisions;
            }

            public void Execute()
            {
                foreach (var (id1, aabb1) in aabbs1.IdsValues)
                {
                    foreach (var (id2, aabb2) in aabbs2.IdsValues)
                    {
                        if (aabb1.Intersects(aabb2))
                        {
                            var edgePair = (collidableEdges1[id1], collidableEdges2[id2]);
                            potentialCollisions.Add(edgePair);
                        }
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
                //dependencies = new CheckForPotentialCollisions(component).Schedule(dependencies);
            }

            return dependencies;
        }
    }
}