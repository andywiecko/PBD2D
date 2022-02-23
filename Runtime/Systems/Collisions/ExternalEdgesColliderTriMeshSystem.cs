using andywiecko.BurstCollections;
using andywiecko.PBD2D.Core;
using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace andywiecko.PBD2D.Systems
{
    [AddComponentMenu("PBD2D:Systems/Collisions/External Edges Collider TriMesh System")]
    public class ExternalEdgesColliderTriMeshSystem : BaseSystem<IExternalEdgesColliderTriMesh>
    {
        [BurstCompile]
        private struct UpdateAABBsJob : IJobParallelFor
        {
            private NativeIndexedArray<Id<ExternalEdge>, AABB> aabbs;
            private NativeIndexedArray<Id<ExternalEdge>, ExternalEdge>.ReadOnly externalEdges;
            private NativeIndexedArray<Id<Point>, float2>.ReadOnly positions;
            private readonly float radius;

            public UpdateAABBsJob(IExternalEdgesColliderTriMesh triMesh)
            {
                aabbs = triMesh.AABBs.Value;
                externalEdges = triMesh.ExternalEdges.Value.AsReadOnly();
                positions = triMesh.PredictedPositions.Value.AsReadOnly();
                radius = triMesh.CollisionRadius + triMesh.Margin;
            }

            public JobHandle Schedule(JobHandle dependencies) => this.Schedule(externalEdges.Length, innerloopBatchCount: 64, dependencies);

            public void Execute(int index)
            {
                var id = (Id<ExternalEdge>)index;
                var edge = externalEdges[id];
                if (!edge.IsEnabled)
                {
                    return;
                }

                var (idA, idB) = edge;
                var (pA, pB) = (positions[idA], positions[idB]);
                aabbs[id] = new AABB
                (
                    min: math.min(pA, pB) - radius,
                    max: math.max(pA, pB) + radius
                );
            }
        }

        public override JobHandle Schedule(JobHandle dependencies)
        {
            foreach (var component in References)
            {
                dependencies = new UpdateAABBsJob(component).Schedule(dependencies);
            }

            return dependencies;
        }
    }
}