using andywiecko.BurstCollections;
using andywiecko.BurstMathUtils;
using andywiecko.ECS;
using andywiecko.PBD2D.Core;
using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;

namespace andywiecko.PBD2D.Systems
{
    [Category(PBDCategory.Collisions)]
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
                previousPositions = pointComponent.Positions.Value.AsReadOnly();
                r = pointComponent.CollisionRadius;
                (p, n) = lineComponent.Line;
                dl = lineComponent.Displacement;
                mu = tuple.Friction;
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

                var dP = -C * n;
                positions[pointId] += dP;

                var (dx, _) = FrictionUtils.GetFrictionCorrections(
                    pA: positions[pointId], qA: previousPositions[pointId], wA: 1,
                    pB: dl, qB: 0, wB: 0,
                    n, mu, fn: dP
                );
                positions[pointId] += dx;
            }

            public JobHandle Schedule(JobHandle dependencies)
            {
                return this.Schedule(positions.Length, innerloopBatchCount: 64, dependencies);
            }
        }

        public override JobHandle Schedule(JobHandle dependencies)
        {
            for (int i = 0; i < References.Count; i++)
            {
                var component = References[i];
                var bounds = component.PointComponent.Bounds;
                var line = component.LineComponent.Line;
                if (!bounds.IsAboveLine(line))
                {
                    dependencies = new PullPointsAboveSurfaceJob(component).Schedule(dependencies);
                }
            }

            return dependencies;
        }
    }
}
