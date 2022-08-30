using andywiecko.BurstCollections;
using andywiecko.BurstMathUtils;
using andywiecko.ECS;
using andywiecko.PBD2D.Core;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace andywiecko.PBD2D.Systems
{
    public abstract class PointTriFieldCollisionSystem<TField> : BaseSystem<IPointTriFieldCollisionTuple<TField>>
        where TField : struct, ITriFieldLookup
    {
        [BurstCompile]
        private struct CheckCollisionJob : IJobParallelForDefer
        {
            [ReadOnly]
            private NativeArray<IdPair<Point, Triangle>> potentialCollisions;

            private NativeList<IdPair<Point, ExternalEdge>>.ParallelWriter collisions;
            private NativeIndexedArray<Id<Triangle>, Triangle>.ReadOnly triangles;
            private NativeIndexedArray<Id<Point>, float2>.ReadOnly pointsPositions;
            private NativeIndexedArray<Id<Point>, float2>.ReadOnly triFieldPositions;
            private readonly TField lookup;

            public CheckCollisionJob(IPointTriFieldCollisionTuple<TField> tuple)
            {
                potentialCollisions = tuple.PotentialCollisions.Value.AsDeferredJobArray();
                collisions = tuple.Collisions.Value.AsParallelWriter();
                triangles = tuple.TriFieldComponent.Triangles.Value.AsReadOnly();
                pointsPositions = tuple.PointsComponent.Positions.Value.AsReadOnly();
                triFieldPositions = tuple.TriFieldComponent.Positions.Value.AsReadOnly();
                lookup = tuple.TriFieldComponent.TriFieldLookup;
            }

            public void Execute(int index)
            {
                var (pId, tId) = potentialCollisions[index];
                var p = pointsPositions[pId];
                var (a, b, c) = triFieldPositions.At(triangles[tId]);
                if (MathUtils.PointInsideTriangle(p, a, b, c, out var bar))
                {
                    var eId = lookup.GetExternalEdge(tId, bar);
                    collisions.AddNoResize((pId, eId));
                }
            }
        }

        [BurstCompile]
        private struct ResolveCollisionsJob : IJob
        {
            [ReadOnly]
            private NativeArray<IdPair<Point, ExternalEdge>> collisions;
            private NativeIndexedArray<Id<Point>, float2> pointPositions;
            private NativeIndexedArray<Id<Point>, float2>.ReadOnly pointPreviousPosisions;
            private NativeIndexedArray<Id<Point>, float>.ReadOnly pointWeights;
            private NativeIndexedArray<Id<Point>, float2> triFieldPositions;
            private NativeIndexedArray<Id<Point>, float2>.ReadOnly triFieldPreviousPositions;
            private NativeIndexedArray<Id<Point>, float>.ReadOnly triFieldWeights;
            private NativeIndexedArray<Id<ExternalEdge>, ExternalEdge>.ReadOnly externalEdges;
            private readonly float mu;

            public ResolveCollisionsJob(IPointTriFieldCollisionTuple<TField> tuple)
            {
                collisions = tuple.Collisions.Value.AsDeferredJobArray();
                pointPositions = tuple.PointsComponent.Positions;
                pointPreviousPosisions = tuple.PointsComponent.PreviousPositions.Value.AsReadOnly();
                pointWeights = tuple.PointsComponent.Weights.Value.AsReadOnly();
                triFieldPositions = tuple.TriFieldComponent.Positions;
                triFieldPreviousPositions = tuple.TriFieldComponent.PreviousPositions.Value.AsReadOnly();
                triFieldWeights = tuple.TriFieldComponent.Weights.Value.AsReadOnly();
                externalEdges = tuple.TriFieldComponent.ExternalEdges.Value.AsReadOnly();
                mu = tuple.Friction;
            }

            public void Execute()
            {
                foreach (var c in collisions)
                {
                    Execute(c);
                }
            }

            private void Execute(IdPair<Point, ExternalEdge> c)
            {
                var (pId, eId) = c;
                var p = pointPositions[pId];
                var wp = pointWeights[pId];
                var e = externalEdges[eId];
                var (e1, e2) = triFieldPositions.At(e);
                var (we1, we2) = triFieldWeights.At(e);

                MathUtils.PointClosestPointOnLineSegment(p, e1, e2, out var q);
                var bar = MathUtils.BarycentricSafe(e1, e2, q, 0.5f);
                var barSq = bar * bar;
                var wq = we1 * barSq.x + we2 * barSq.y;
                var wpq = wp + wq;

                if (wpq == 0)
                {
                    return;
                }

                var n = e.GetNormal(triFieldPositions);

                if (math.dot(p - q, n) > math.EPSILON)
                {
                    return;
                }

                var dx = (p - q) / wpq;

                pointPositions[pId] -= wp * dx;
                triFieldPositions[e.IdA] += bar.x * we1 * dx;
                triFieldPositions[e.IdB] += bar.y * we2 * dx;

                var (dp1, dp2) = FrictionUtils.GetFrictionCorrections(
                    pA: pointPositions[pId],
                    qA: pointPreviousPosisions[pId],
                    wp,
                    pB: bar.x * triFieldPositions[e.IdA] + bar.y * triFieldPositions[e.IdB],
                    qB: bar.x * triFieldPreviousPositions[e.IdA] + bar.y * triFieldPreviousPositions[e.IdB],
                    wq,
                    n, mu, fn: p - q);

                pointPositions[pId] += dp1;

                if (wq != 0)
                {
                    triFieldPositions[e.IdA] += bar.x * we1 / wq * dp2;
                    triFieldPositions[e.IdB] += bar.y * we2 / wq * dp2;
                }
            }
        }

        public override JobHandle Schedule(JobHandle dependencies)
        {
            foreach (var component in References)
            {
                if (component.Intersecting())
                {
                    dependencies = new CommonJobs.ClearListJob<IdPair<Point, ExternalEdge>>(component.Collisions.Value).Schedule(dependencies);
                    dependencies = new CheckCollisionJob(component).Schedule(component.PotentialCollisions.Value, innerloopBatchCount: 64, dependencies);
                    dependencies = new ResolveCollisionsJob(component).Schedule(dependencies);
                }
            }

            return dependencies;
        }
    }
}
