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
        [BurstCompile]
        private struct DetectCollisionsJob : IJobParallelForDefer
        {
            [ReadOnly]
            private NativeArray<IdPair<Edge>> potentialCollisions;
            private NativeList<EdgeEdgeContactInfo>.ParallelWriter collisions;
            private NativeIndexedArray<Id<Edge>, Edge>.ReadOnly edges1;
            private NativeIndexedArray<Id<Edge>, Edge>.ReadOnly edges2;
            private NativeIndexedArray<Id<Point>, float2>.ReadOnly positions1;
            private NativeIndexedArray<Id<Point>, float2>.ReadOnly positions2;
            private readonly float contactRadiusSq;

            public DetectCollisionsJob(ICapsuleCapsuleCollisionTuple tuple)
            {
                var (triMesh1, triMesh2) = tuple;
                edges1 = triMesh1.Edges.Value.AsReadOnly();
                edges2 = triMesh2.Edges.Value.AsReadOnly();
                positions1 = triMesh1.PredictedPositions.Value.AsReadOnly();
                positions2 = triMesh2.PredictedPositions.Value.AsReadOnly();
                var radius1 = triMesh1.CollisionRadius;
                var radius2 = triMesh2.CollisionRadius;
                contactRadiusSq = (radius1 + radius2) * (radius1 + radius2);

                potentialCollisions = tuple.PotentialCollisions.Value.AsDeferredJobArray();
                collisions = tuple.Collisions.Value.AsParallelWriter();
            }

            public void Execute(int i)
            {
                var (e1Id, e2Id) = potentialCollisions[i];
                var (a0, a1) = positions1.At(edges1[e1Id]);
                var (b0, b1) = positions2.At(edges2[e2Id]);

                MathUtils.ShortestLineSegmentBetweenLineSegments(a0, a1, b0, b1, out var pA, out var pB);

                if (math.distancesq(pA, pB) <= 4 * contactRadiusSq)
                {
                    var barA = MathUtils.BarycentricSafe(a0, a1, pA, 0.5f);
                    var barB = MathUtils.BarycentricSafe(b0, b1, pB, 0.5f);
                    collisions.AddNoResize(new(barA, barB, e1Id, e2Id));
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
            private NativeIndexedArray<Id<Point>, float2>.ReadOnly previousPositions1;
            private NativeIndexedArray<Id<Point>, float2>.ReadOnly previousPositions2;
            private readonly float mu;

            [ReadOnly]
            private NativeArray<EdgeEdgeContactInfo> collisions;

            private readonly float contactRadius;

            public ResolveCollisionsJob(ICapsuleCapsuleCollisionTuple tuple)
            {
                var (triMesh1, triMesh2) = tuple;
                edges1 = triMesh1.Edges.Value.AsReadOnly();
                edges2 = triMesh2.Edges.Value.AsReadOnly();
                positions1 = triMesh1.PredictedPositions;
                positions2 = triMesh2.PredictedPositions;
                massesInv1 = triMesh1.MassesInv.Value.AsReadOnly();
                massesInv2 = triMesh2.MassesInv.Value.AsReadOnly();
                previousPositions1 = triMesh1.Positions.Value.AsReadOnly();
                previousPositions2 = triMesh2.Positions.Value.AsReadOnly();
                mu = tuple.Friction;
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
                var (barA, barB, edgeIdA, edgeIdB) = contactInfo;
                var ((a0Id, a1Id), (b0Id, b1Id)) = (edges1[edgeIdA], edges2[edgeIdB]);

                var (a0, a1) = (positions1[a0Id], positions1[a1Id]);
                var pA = a0 * barA.x + a1 * barA.y;
                var (b0, b1) = (positions2[b0Id], positions2[b1Id]);
                var pB = b0 * barB.x + b1 * barB.y;

                var distance = math.distance(pA, pB);
                if (distance <= math.EPSILON)
                {
                    return;
                }

                var C = distance - contactRadius;
                if (C >= 0)
                {
                    return;
                }

                var n = math.normalize(pA - pB);

                var (wa0, wa1, wb0, wb1) = (massesInv1[a0Id], massesInv1[a1Id], massesInv2[b0Id], massesInv2[b1Id]);
                var barASq = barA * barA;
                var barBSq = barB * barB;
                var lambda = wa0 * barASq.x + wa1 * barASq.y + wb0 * barBSq.x + wb1 * barBSq.y;
                if (lambda == 0)
                {
                    return;
                }

                var stiffness = 1f;
                lambda = stiffness * C / lambda;

                positions1[a0Id] -= lambda * wa0 * barA.x * n;
                positions1[a1Id] -= lambda * wa1 * barA.y * n;
                positions2[b0Id] += lambda * wb0 * barB.x * n;
                positions2[b1Id] += lambda * wb1 * barB.y * n;

                var wA = wa0 * barASq.x + wa1 * barASq.y;
                var wB = wb0 * barBSq.x + wb1 * barBSq.y;

                var (dp1, dp2) = FrictionUtils.GetFrictionCorrections(
                    pA: positions1[a0Id] * barA.x + positions1[a1Id] * barA.y,
                    qA: previousPositions1[a0Id] * barA.x + previousPositions1[a1Id] * barA.y,
                    wA,

                    pB: positions2[b0Id] * barB.x + positions2[b1Id] * barB.y,
                    qB: previousPositions2[b0Id] * barB.x + previousPositions2[b1Id] * barB.y,
                    wB,
                    n, mu, fn: math.float2(C, 0));

                positions1[a0Id] += barA.x * wa0 / wA * dp1;
                positions1[a1Id] += barA.y * wa1 / wA * dp1;
                positions2[b0Id] += barB.x * wb0 / wB * dp2;
                positions2[b1Id] += barB.y * wb1 / wB * dp2;
            }
        }

        public override JobHandle Schedule(JobHandle dependencies = default)
        {
            foreach (var component in References)
            {
                dependencies = new CommonJobs.ClearListJob<EdgeEdgeContactInfo>(component.Collisions).Schedule(dependencies);
                dependencies = new DetectCollisionsJob(component).Schedule(component.PotentialCollisions.Value, innerloopBatchCount: 64, dependencies);
                dependencies = new ResolveCollisionsJob(component).Schedule(dependencies);
            }

            return dependencies;
        }
    }
}