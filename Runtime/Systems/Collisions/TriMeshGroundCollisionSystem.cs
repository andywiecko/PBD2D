using andywiecko.BurstCollections;
using andywiecko.BurstMathUtils;
using andywiecko.PBD2D.Core;
using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace andywiecko.PBD2D.Systems
{
    [AddComponentMenu("PBD2D:Systems/Collisions/TriMesh Ground Collision System")]
    public class TriMeshGroundCollisionSystem : BaseSystem<ITriMeshGroundCollisionTuple>
    {
        [BurstCompile]
        private struct PullPointsAboveSurfaceJob : IJobParallelFor
        {
            private NativeIndexedArray<Id<Point>, float2> positions;
            private readonly float2 surfacePoint;
            private readonly float2 surfaceNormal;

            public PullPointsAboveSurfaceJob(ITriMeshGroundCollisionTuple tuple)
            {
                var (triMesh, ground) = tuple;

                positions = triMesh.PredictedPositions;
                (surfacePoint, surfaceNormal) = ground.Surface;
            }

            public void Execute(int index)
            {
                var pointId = (Id<Point>)index;
                var position = positions[pointId];
                var signedDistance = MathUtils.PointLineSignedDistance(position, surfaceNormal, surfacePoint);

                if (signedDistance >= 0)
                {
                    return;
                }

                var dP = signedDistance * surfaceNormal;
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