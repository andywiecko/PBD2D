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
            private readonly float2 p;
            private readonly float2 n;
            private readonly float r;

            public PullPointsAboveSurfaceJob(IPointLineCollisionTuple tuple)
            {
                var (pointComponent, lineComponent) = tuple;

                positions = pointComponent.PredictedPositions;
                r = pointComponent.CollisionRadius;
                (p, n) = lineComponent.Line;
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
