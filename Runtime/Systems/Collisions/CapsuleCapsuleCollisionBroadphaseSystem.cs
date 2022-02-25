using andywiecko.BurstCollections;
using andywiecko.PBD2D.Core;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace andywiecko.PBD2D.Systems
{
    [AddComponentMenu("PBD2D:Systems/Collisions/Capsule Capsule Collision System (Broadphase)")]
    public class CapsuleCapsuleCollisionBroadphaseSystem : BaseSystem<ICapsuleCapsuleCollisionTuple>
    {
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
                potentialCollisions.Clear();

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
                dependencies = new CheckForPotentialCollisions(component).Schedule(dependencies);
            }

            return dependencies;
        }
    }
}