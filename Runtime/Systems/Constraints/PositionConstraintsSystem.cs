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
    public class PositionConstraintsSystem : BaseSystemWithConfiguration<IPositionConstraints, PBDConfiguration>
    {
        [BurstCompile]
        private struct ApplyConstraintJob : IJob
        {
            private readonly float a;
            private readonly float k;
            [ReadOnly]
            private NativeArray<PositionConstraint> constraints;
            private NativeIndexedArray<Id<Point>, float2> positions;
            private NativeIndexedArray<Id<Point>, float>.ReadOnly weights;

            public ApplyConstraintJob(IPositionConstraints component, float dt)
            {
                a = component.Compliance / dt / dt;
                k = component.Stiffness;
                constraints = component.Constraints.Value.AsDeferredJobArray();
                positions = component.Positions;
                weights = component.Weights.Value.AsReadOnly();
            }

            public void Execute()
            {
                foreach (var c in constraints)
                {
                    Execute(c);
                }
            }

            private void Execute(PositionConstraint c)
            {
                var w = weights.At(c);
                if (w == 0)
                {
                    return;
                }

                var p = positions.At(c);
                var q = c.Position;
                var dx = -k * w / (w + a) * (p - q);
                positions[c.Id] += dx;
            }
        }

        public override JobHandle Schedule(JobHandle dependencies)
        {
            foreach (var component in References)
            {
                dependencies = new ApplyConstraintJob(component, Configuration.ReducedDeltaTime).Schedule(dependencies);
            }

            return dependencies;
        }
    }
}