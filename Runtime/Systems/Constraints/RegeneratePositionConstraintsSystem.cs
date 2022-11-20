using andywiecko.ECS;
using andywiecko.PBD2D.Core;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace andywiecko.PBD2D.Systems
{
    [Category(PBDCategory.Constraints)]
    public class RegeneratePositionConstraintsSystem : BaseSystem<IRegeneratePositionConstraints>
    {
        [BurstCompile]
        private struct RegenerateConstraintsJob : IJobParallelForDefer
        {
            [ReadOnly]
            private NativeArray<float2> initialRelativePositions;

            private NativeArray<PositionConstraint> constraints;
            private readonly float2 position;

            public RegenerateConstraintsJob(IRegeneratePositionConstraints component)
            {
                initialRelativePositions = component.InitialRelativePositions.Value.AsDeferredJobArray();
                constraints = component.Constraints.Value.AsDeferredJobArray();
                position = component.TransformPosition;
            }

            public void Execute(int i)
            {
                var c = constraints[i];
                constraints[i] = c.With(position + initialRelativePositions[i]);
            }
        }

        public override JobHandle Schedule(JobHandle dependencies)
        {
            foreach (var c in References)
            {
                if (c.TransformChanged)
                {
                    dependencies = new RegenerateConstraintsJob(c).Schedule(c.Constraints.Value, innerloopBatchCount: 64, dependencies);
                }
            }

            return dependencies;
        }
    }
}