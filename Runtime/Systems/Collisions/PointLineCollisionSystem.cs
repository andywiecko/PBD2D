using andywiecko.BurstCollections;
using andywiecko.BurstMathUtils;
using andywiecko.PBD2D.Core;
using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace andywiecko.PBD2D.Systems
{
    [AddComponentMenu("PBD2D:Systems/Collisions/Point Line Collision System")]
    public class PointLineCollisionSystem : BaseSystem<IPointLineCollisionTuple>
    {
        [BurstCompile]
        private struct PullPointsAboveSurfaceJob : IJobParallelFor
        {
            private NativeIndexedArray<Id<Point>, float2> positions;
            private NativeIndexedArray<Id<Point>, float2>.ReadOnly previousPositions;
            private readonly float2 dl;
            private readonly float mu;
            private readonly float2 p;
            private readonly float2 n;
            private readonly float r;

            public PullPointsAboveSurfaceJob(IPointLineCollisionTuple tuple)
            {
                var (pointComponent, lineComponent) = tuple;

                positions = pointComponent.PredictedPositions;
                previousPositions = pointComponent.Positions;
                r = pointComponent.CollisionRadius;
                (p, n) = lineComponent.Line;
                dl = lineComponent.Displacement;
                mu = pointComponent.Friction;
            }

            public void Execute(int index)
            {
                var pointId = (Id<Point>)index;
                var position = positions[pointId];
                var signedDistance = MathUtils.PointLineSignedDistance(position, n, p);
                var C = signedDistance - r;

                if (C >= 0)
                {
                    return;
                }

                var dP = C * n;
                positions[pointId] -= dP;

                // friction
                var dx = (positions[pointId] - previousPositions[pointId]) - dl;
                var dxn = n * math.dot(n, dx);
                var dxt = dx - dxn;
                dx = math.min(math.length(dxt), mu * math.length(dP)) * math.normalizesafe(dxt);
                positions[pointId] -= dx;
            }

            public JobHandle Schedule(JobHandle dependencies)
            {
                return this.Schedule(positions.Length, innerloopBatchCount: 64, dependencies);
            }
        }

        public override JobHandle Schedule(JobHandle dependencies)
        {
            foreach (var tuple in References)
            {
                dependencies = new PullPointsAboveSurfaceJob(tuple).Schedule(dependencies);
            }

            return dependencies;
        }
    }
}
