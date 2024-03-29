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
    public class TriangleAreaConstraintsSystem : BaseSystemWithConfiguration<ITriangleAreaConstraints, PBDConfiguration>
    {
        [BurstCompile]
        private struct ApplyAreaConstraintJob : IJob
        {
            private readonly float a;
            private readonly float k;
            private NativeIndexedArray<Id<Point>, float2> positions;
            private NativeIndexedArray<Id<Point>, float>.ReadOnly weights;
            [ReadOnly]
            private NativeArray<TriangleAreaConstraint> constraints;

            public ApplyAreaConstraintJob(ITriangleAreaConstraints component, float dt)
            {
                a = component.Compliance / dt / dt;
                k = component.Stiffness;
                positions = component.Positions;
                weights = component.Weights.Value.AsReadOnly();
                constraints = component.Constraints.Value.AsDeferredJobArray();
            }

            public void Execute()
            {
                foreach (var c in constraints)
                {
                    ApplyAreaConstraint(c);
                }
            }

            private void ApplyAreaConstraint(TriangleAreaConstraint c)
            {
                var (idA, idB, idC, restArea2) = c;
                var (pA, pB, pC) = positions.At(c);
                var (wA, wB, wC) = weights.At(c);

                var pAB = pB - pA;
                var pAC = pC - pA;
                var pBC = pC - pB;

                var lambda = wA * math.lengthsq(pBC) + wB * math.lengthsq(pAC) + wC * math.lengthsq(pAB) + a;
                if (lambda < math.EPSILON)
                {
                    return;
                }
                lambda = k / lambda;

                var A = MathUtils.Cross(pAB, pAC);
                if (math.abs(A) < math.EPSILON)
                {
                    return;
                }

                var C = A - restArea2;
                positions[idA] += lambda * wA * C * MathUtils.Rotate90CW(pBC);
                positions[idB] += lambda * wB * C * MathUtils.Rotate90CW(-pAC);
                positions[idC] += lambda * wC * C * MathUtils.Rotate90CW(pAB);
            }
        }

        public override JobHandle Schedule(JobHandle dependencies = default)
        {
            foreach (var component in References)
            {
                if (component.Stiffness != 0)
                {
                    dependencies = new ApplyAreaConstraintJob(component, Configuration.ReducedDeltaTime).Schedule(dependencies);
                }
            }

            return dependencies;
        }
    }
}