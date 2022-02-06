using andywiecko.BurstCollections;
using andywiecko.BurstMathUtils;
using andywiecko.PBD2D.Core;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace andywiecko.PBD2D.Systems
{
    [AddComponentMenu("PBD2D:Systems/Collisions/Capsule Capsule Collision System")]
    public class CapsuleCapsuleCollisionSystem : BaseSystem<ICapsuleCapsuleCollisionTuple>
    {
        // Use BVT to speed up
        // Make this as broadphase at interframe somehow.
        [BurstCompile]
        private struct CheckForPotentialCollisions : IJob
        {
            private NativeIndexedArray<Id<CollidableEdge>, AABB>.ReadOnly aabbs1;
            private NativeIndexedArray<Id<CollidableEdge>, AABB>.ReadOnly aabbs2;
            private NativeIndexedArray<Id<CollidableEdge>, Id<Edge>>.ReadOnly collidableEdges1;
            private NativeIndexedArray<Id<CollidableEdge>, Id<Edge>>.ReadOnly collidableEdges2;
            private NativeList<EdgePair> potentialCollisions;

            public CheckForPotentialCollisions(ICapsuleCapsuleCollisionTuple tuple)
            {
                var (c1, c2) = tuple;
                aabbs1 = c1.AABBs;
                aabbs2 = c2.AABBs;
                collidableEdges1 = c1.CollidableEdges;
                collidableEdges2 = c2.CollidableEdges;

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

        [BurstCompile]
        private struct DetectCollisionsJob : IJob
        {
            [ReadOnly]
            private NativeArray<EdgePair> potentialCollisions;
            private NativeList<EdgeEdgeContactInfo> collisions;
            private NativeIndexedArray<Id<Edge>, Edge>.ReadOnly edges1;
            private NativeIndexedArray<Id<Edge>, Edge>.ReadOnly edges2;
            private NativeIndexedArray<Id<Point>, float2> positions1;
            private NativeIndexedArray<Id<Point>, float2> positions2;
            private readonly float contactRadiusSq;

            public DetectCollisionsJob(ICapsuleCapsuleCollisionTuple tuple)
            {
                var (triMesh1, triMesh2) = tuple;
                edges1 = triMesh1.Edges;
                edges2 = triMesh2.Edges;
                positions1 = triMesh1.PredictedPositions;
                positions2 = triMesh2.PredictedPositions;
                var radius1 = triMesh1.CollisionRadius;
                var radius2 = triMesh2.CollisionRadius;
                contactRadiusSq = (radius1 + radius2) * (radius1 + radius2);

                potentialCollisions = tuple.PotentialCollisions.Value.AsDeferredJobArray();
                collisions = tuple.Collisions;
            }

            public void Execute()
            {
                collisions.Clear();

                foreach (var (e1Id, e2Id) in potentialCollisions)
                {
                    var (a0Id, a1Id) = edges1[e1Id];
                    var (b0Id, b1Id) = edges2[e2Id];
                    var (a0, a1, b0, b1) = (positions1[a0Id], positions1[a1Id], positions2[b0Id], positions2[b1Id]);

                    MathUtils.ShortestLineSegmentBetweenLineSegments(a0, a1, b0, b1, out var pA, out var pB);

                    if (math.distancesq(pA, pB) <= 4 * contactRadiusSq)
                    {
                        collisions.Add(new(pA, pB, e1Id, e2Id));
                    }
                }
            }
        }

        [BurstCompile]
        private struct ResolveCollisionsJob : IJob
        {
            private NativeIndexedArray<Id<Edge>, Edge>.ReadOnly edges1;
            private NativeIndexedArray<Id<Edge>, Edge>.ReadOnly edges2;
            private NativeIndexedArray<Id<Point>, float2> positions1;
            private NativeIndexedArray<Id<Point>, float2> positions2;
            private NativeIndexedArray<Id<Point>, float>.ReadOnly massesInv1;
            private NativeIndexedArray<Id<Point>, float>.ReadOnly massesInv2;
            private NativeIndexedArray<Id<Point>, Friction> friction1;
            private NativeIndexedArray<Id<Point>, Friction> friction2;
            [ReadOnly]
            private NativeArray<EdgeEdgeContactInfo> collisions;

            private readonly float contactRadius;

            public ResolveCollisionsJob(ICapsuleCapsuleCollisionTuple tuple)
            {
                var (triMesh1, triMesh2) = tuple;
                edges1 = triMesh1.Edges;
                edges2 = triMesh2.Edges;
                positions1 = triMesh1.PredictedPositions;
                positions2 = triMesh2.PredictedPositions;
                massesInv1 = triMesh1.MassesInv;
                massesInv2 = triMesh2.MassesInv;
                friction1 = triMesh1.AccumulatedFriction;
                friction2 = triMesh2.AccumulatedFriction;

                contactRadius = triMesh1.CollisionRadius + triMesh2.CollisionRadius;

                collisions = tuple.Collisions.Value.AsDeferredJobArray();
            }

            public void Execute()
            {
                foreach (var contactInfo in collisions)
                {
                    ResolveCollision(contactInfo);
                }
            }

            private void ResolveCollision(EdgeEdgeContactInfo contactInfo)
            {
                var (pA, pB, edgeIdA, edgeIdB) = contactInfo;

                var ((a0Id, a1Id), (b0Id, b1Id)) = (edges1[edgeIdA], edges2[edgeIdB]);

                var distance = math.distance(pA, pB);
                var n = math.normalizesafe(pA - pB, math.float2(0, 1));
                var C = distance - contactRadius;

                if (C >= 0)
                {
                    return;
                }

                float s, t;
                var a0a1 = math.distance(positions1[a0Id], positions1[a1Id]);
                var a0pA = math.distance(positions1[a0Id], pA);
                if (a0pA != 0f)
                {
                    s = a0pA / a0a1;
                }
                else
                {
                    s = 0;
                }

                var b0b1 = math.distance(positions2[b0Id], positions2[b1Id]);
                var b0pB = math.distance(positions2[b0Id], pB);
                if (b0pB != 0f)
                {
                    t = b0pB / b0b1;
                }
                else
                {
                    t = 0;
                }

                s = math.clamp(s, 0, 1);
                t = math.clamp(t, 0, 1);

                var a0grad = n * (1f - s);
                var a1grad = n * s;
                var b0grad = -n * (1f - t);
                var b1grad = -n * t;

                var (a0mInv, a1mInv) = (massesInv1[a0Id], massesInv1[a1Id]);
                var (b0mInv, b1mInv) = (massesInv2[b0Id], massesInv2[b1Id]);

                var lambda = a0mInv * (1 - s) * (1 - s) + a1mInv * s * s + b0mInv * (1 - t) * (1 - t) + b1mInv * t * t;
                if (lambda == 0.0)
                {
                    return;
                }

                var stiffness = 1f;
                lambda = stiffness * C / lambda;

                positions1[a0Id] -= lambda * a0mInv * a0grad;
                positions1[a1Id] -= lambda * a1mInv * a1grad;
                positions2[b0Id] -= lambda * b0mInv * b0grad;
                positions2[b1Id] -= lambda * b1mInv * b1grad;

                // TODO: coeficients
                friction1[a0Id] += new Friction(lambda * a0mInv * a0grad);
                friction1[a1Id] += new Friction(lambda * a1mInv * a1grad);
                friction2[b0Id] += new Friction(lambda * b0mInv * b0grad);
                friction2[b1Id] += new Friction(lambda * b1mInv * b1grad);
            }
        }

        public override JobHandle Schedule(JobHandle dependencies)
        {
            foreach (var component in References)
            {
                dependencies = new CheckForPotentialCollisions(component).Schedule(dependencies);
                dependencies = new DetectCollisionsJob(component).Schedule(dependencies);
                dependencies = new ResolveCollisionsJob(component).Schedule(dependencies);
            }

            return dependencies;
        }
    }
}