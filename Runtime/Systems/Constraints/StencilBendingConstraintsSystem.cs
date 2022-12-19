using andywiecko.BurstCollections;
using andywiecko.BurstMathUtils;
using andywiecko.ECS;
using andywiecko.PBD2D.Core;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace andywiecko.PBD2D.Systems
{
    [Category(PBDCategory.Constraints)]
    public class StencilBendingConstraintsSystem : BaseSystemWithConfiguration<IStencilBendingConstraints, PBDConfiguration>
    {
        [BurstCompile]
        private struct ApplyBendingConstraintJob : IJob
        {
            private readonly float k;
            private readonly float a;
            [ReadOnly]
            private NativeArray<StencilBendingConstraint> constraints;
            private NativeIndexedArray<Id<Point>, float>.ReadOnly weights;
            private NativeIndexedArray<Id<Point>, float2> positions;

            public ApplyBendingConstraintJob(IStencilBendingConstraints component, float dt)
            {
                k = component.Stiffness;
                a = component.Compliance / dt / dt;
                constraints = component.Constraints.Value.AsDeferredJobArray();
                weights = component.Weights.Value.AsReadOnly();
                positions = component.Positions;
            }

            public void Execute()
            {
                foreach (var c in constraints)
                {
                    Execute(c);
                }
            }

            private void Execute(StencilBendingConstraint c)
            {
                var (pA, pB, pC) = positions.At(c);
                var (wA, wB, wC) = weights.At(c);
                var (t1, t2, t3) = (pB - pA, pC - pB, pC - pA);

                var w = wA * math.lengthsq(t2) + wB * math.lengthsq(t3) + wC * math.lengthsq(t1);
                if (w <= math.EPSILON)
                {
                    return;
                }

                var lambda = 1 / (w + a);
                var cross = MathUtils.Cross(t1, t2);
                var dot = math.dot(t1, t2);
                var C = math.atan2(cross, dot) - c.RestAngle;

                positions[c.IdA] += -k * wA * lambda * C * (MathUtils.Rotate90CW(-t2) * dot + cross * t2);
                positions[c.IdB] += -k * wB * lambda * C * (MathUtils.Rotate90CW(t1 + t2) * dot - cross * (t2 - t1));
                positions[c.IdC] += -k * wC * lambda * C * (MathUtils.Rotate90CW(-t1) * dot - cross * t1);
            }
        }

        public override JobHandle Schedule(JobHandle dependencies)
        {
            foreach (var component in References)
            {
                if (component.Stiffness != 0)
                {
                    dependencies = new ApplyBendingConstraintJob(component, Configuration.ReducedDeltaTime).Schedule(dependencies);
                }
            }

            return dependencies;
        }
    }
}