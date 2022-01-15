using andywiecko.BurstCollections;
using andywiecko.PBD2D.Core;
using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace andywiecko.PBD2D.Systems
{
    [AddComponentMenu("PBD2D:Systems/Collisions/Triangles Collider TriMesh System")]
    public class TrianglesColliderTriMeshSystem : BaseSystem<ITrianglesColliderTriMesh>
    {
        [BurstCompile]
        private struct UpdateAABBsJob : IJobParallelFor
        {
            private NativeIndexedArray<Id<Triangle>, AABB> aabbs;
            private NativeIndexedArray<Id<Triangle>, Triangle>.ReadOnly triangles;
            private NativeIndexedArray<Id<Point>, float2>.ReadOnly positions;
            private readonly float margin;

            public UpdateAABBsJob(ITrianglesColliderTriMesh triMesh)
            {
                aabbs = triMesh.AABBs.Value;
                triangles = triMesh.Triangles;
                positions = triMesh.PredictedPositions;
                margin = triMesh.Margin;
            }

            public JobHandle Schedule(JobHandle dependencies) => this.Schedule(triangles.Length, innerloopBatchCount: 64, dependencies);

            public void Execute(int index)
            {
                var id = (Id<Triangle>)index;
                var triangle = triangles[id];
                if (!triangle.IsEnabled)
                {
                    return;
                }

                aabbs[id] = triangle.ToAABB(positions, margin);
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
