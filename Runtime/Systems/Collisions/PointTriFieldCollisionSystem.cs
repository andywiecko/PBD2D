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
    [Category("Collisions")]
    public class PointTriFieldCollisionSystem : BaseSystem<IPointTriFieldCollisionTuple>
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
            private TriFieldLookup.ReadOnly lookup;

            public CheckCollisionJob(IPointTriFieldCollisionTuple tuple)
            {
                potentialCollisions = tuple.PotentialCollisions.Value.AsDeferredJobArray();
                collisions = tuple.Collisions.Value.AsParallelWriter();
                triangles = tuple.TriFieldComponent.Triangles.Value.AsReadOnly();
                pointsPositions = tuple.PointsComponent.PredictedPositions.Value.AsReadOnly();
                triFieldPositions = tuple.TriFieldComponent.PredictedPositions.Value.AsReadOnly();
                lookup = tuple.TriFieldComponent.TriFieldLookup.Value.AsReadOnly();
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
            private NativeIndexedArray<Id<Point>, float>.ReadOnly pointMassesInv;
            private NativeIndexedArray<Id<Point>, float2> triFieldPositions;
            private NativeIndexedArray<Id<Point>, float>.ReadOnly triFieldMassesInv;
            private NativeIndexedArray<Id<ExternalEdge>, ExternalEdge>.ReadOnly externalEdges;

            public ResolveCollisionsJob(IPointTriFieldCollisionTuple tuple)
            {
                collisions = tuple.Collisions.Value.AsDeferredJobArray();
                pointPositions = tuple.PointsComponent.PredictedPositions;
                pointMassesInv = tuple.PointsComponent.MassesInv.Value.AsReadOnly();
                triFieldPositions = tuple.TriFieldComponent.PredictedPositions;
                triFieldMassesInv = tuple.TriFieldComponent.MassesInv.Value.AsReadOnly();
                externalEdges = tuple.TriFieldComponent.ExternalEdges.Value.AsReadOnly();
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
                var wp = pointMassesInv[pId];
                var e = externalEdges[eId];
                var (e1, e2) = triFieldPositions.At(e);
                var (we1, we2) = triFieldMassesInv.At(e);

                MathUtils.PointClosestPointOnLineSegment(p, e1, e2, out var q);
                var bar = MathUtils.BarycentricSafe(q, e1, e2, 0.5f);
                var barSq = bar * bar;
                var wpq = wp + we1 * barSq.x + we2 * barSq.y;

                if (wpq == 0)
                {
                    return;
                }

                var n = e.GetNormal(triFieldPositions);

                const float eps = 1e-9f;
                if (math.dot(p - q, n) > eps)
                {
                    return;
                }

                var dp = (p - q + n * eps) / wpq;

                pointPositions[pId] -= wp * dp;
                triFieldPositions[e.IdA] += we1 * bar.x * dp;
                triFieldPositions[e.IdB] += we2 * bar.y * dp;

                // TODO: friction + tests
            }
        }

        public override JobHandle Schedule(JobHandle dependencies)
        {
            foreach (var component in References)
            {
                dependencies = new CommonJobs.ClearListJob<IdPair<Point, ExternalEdge>>(component.Collisions.Value).Schedule(dependencies);
                dependencies = new CheckCollisionJob(component).Schedule(component.PotentialCollisions.Value, innerloopBatchCount: 64, dependencies);
                dependencies = new ResolveCollisionsJob(component).Schedule(dependencies);
            }

            return dependencies;
        }
    }
}