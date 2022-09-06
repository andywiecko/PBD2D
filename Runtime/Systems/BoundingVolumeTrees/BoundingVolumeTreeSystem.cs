using andywiecko.BurstCollections;
using andywiecko.ECS;
using andywiecko.PBD2D.Core;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace andywiecko.PBD2D.Systems
{
    public abstract class BoundingVolumeTreeSystem<TObject> : BaseSystem<IBoundingVolumeTreeComponent<TObject>>
        where TObject : struct, IConvertableToAABB
    {
        [BurstCompile]
        private struct UpdateAABBsJob : IJobParallelFor
        {
            private NativeArray<AABB> aabbs;
            private NativeArray<TObject>.ReadOnly objects;
            private NativeIndexedArray<Id<Point>, float2>.ReadOnly positions;
            private readonly float margin;

            public UpdateAABBsJob(IBoundingVolumeTreeComponent<TObject> component)
            {
                aabbs = component.Volumes.Value;
                objects = component.Objects.Value.AsReadOnly();
                positions = component.Positions.Value.AsReadOnly();
                margin = component.Margin;
            }

            public JobHandle Schedule(JobHandle dependencies)
            {
                return this.Schedule(aabbs.Length, innerloopBatchCount: 64, dependencies);
            }

            public void Execute(int i)
            {
                aabbs[i] = objects[i].ToAABB(positions, margin);
            }
        }

        public override JobHandle Schedule(JobHandle dependencies)
        {
            for (int i = 0; i < References.Count; i++)
            {
                var component = References[i];
                dependencies = new UpdateAABBsJob(component).Schedule(dependencies);
                dependencies = component.Tree.Value.UpdateLeavesVolumes(component.Volumes.Value.AsReadOnly(), dependencies);
            }

            return dependencies;
        }
    }
}