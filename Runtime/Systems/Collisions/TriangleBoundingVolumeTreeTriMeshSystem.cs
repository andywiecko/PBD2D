using andywiecko.PBD2D.Core;
using UnityEngine;
using Unity.Jobs;
using Unity.Burst;
using Unity.Mathematics;
using andywiecko.BurstCollections;

namespace andywiecko.PBD2D.Systems
{
    [AddComponentMenu("PBD2D:Systems/Extended Data/Triangle Bounding Volume Tree TriMesh System")]
    public class TriangleBoundingVolumeTreeTriMeshSystem : BaseSystem<ITriangleBoundingVolumeTreeTriMesh>
    {
        [BurstCompile]
        private struct UpdateAABBsJob : IJobParallelFor
        {
            private NativeIndexedArray<Id<Triangle>, AABB> aabbs;
            private NativeIndexedArray<Id<Triangle>, Triangle>.ReadOnly triangles;
            private NativeIndexedArray<Id<Point>, float2>.ReadOnly positions;
            private readonly float margin;

            public UpdateAABBsJob(ITriangleBoundingVolumeTreeTriMesh component)
            {
                aabbs = component.AABBs;
                triangles = component.Triangles.Value.AsReadOnly();
                positions = component.Positions.Value.AsReadOnly();
                margin = component.Margin;
            }

            public JobHandle Schedule(JobHandle dependencies)
            {
                return this.Schedule(triangles.Length, innerloopBatchCount: 64, dependencies);
            }

            public void Execute(int index)
            {
                var id = (Id<Triangle>)index;
                aabbs[id] = triangles[id].ToAABB(positions, margin);
                //aabbs[id] = triangles[id].ToBoundingCircle(positions, margin);
            }
        }

        public override JobHandle Schedule(JobHandle dependencies)
        {
            foreach (var component in References)
            {
                dependencies = new UpdateAABBsJob(component).Schedule(dependencies);
                dependencies = component.Tree.Value.UpdateLeavesVolumes(
                    volumes: component.AABBs.Value.GetInnerArray().AsReadOnly(), dependencies);
            }

            return dependencies;
        }
    }
}
