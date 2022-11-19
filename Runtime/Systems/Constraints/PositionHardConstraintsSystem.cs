using andywiecko.BurstCollections;
using andywiecko.ECS;
using andywiecko.PBD2D.Core;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace andywiecko.PBD2D.Systems
{
    [Category(PBDCategory.Constraints)]
    public class PositionHardConstraintsSystem : BaseSystem<IPositionHardConstraints>
    {
        [BurstCompile]
        private struct ApplyConstraintJob : IJob
        {
            [ReadOnly]
            private NativeArray<PositionConstraint> constraints;
            private NativeIndexedArray<Id<Point>, float2> positions;

            public ApplyConstraintJob(IPositionHardConstraints component)
            {
                constraints = component.Constraints.Value.AsDeferredJobArray();
                positions = component.Positions;
            }

            public void Execute()
            {
                foreach (var c in constraints)
                {
                    positions[c.Id] = c.Position;
                }
            }
        }

        public override JobHandle Schedule(JobHandle dependencies)
        {
            foreach (var component in References)
            {
                dependencies = new ApplyConstraintJob(component).Schedule(dependencies);
            }

            return dependencies;
        }
    }
}