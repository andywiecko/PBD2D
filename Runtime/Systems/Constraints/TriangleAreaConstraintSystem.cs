using andywiecko.BurstCollections;
using andywiecko.BurstMathUtils;
using andywiecko.PBD2D.Core;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace andywiecko.PBD2D.Systems
{
    [AddComponentMenu("PBD2D:Systems/Constraints/Triangle Area Constraint System")]
    public class TriangleAreaConstraintSystem : BaseSystem<ITriangleAreaConstraint>
    {
        [BurstCompile]
        private struct ApplyAreaConstraintJob : IJob
        {
            private readonly float stiffness;
            private NativeIndexedArray<Id<Point>, float2> positions;
            private NativeIndexedArray<Id<Point>, float>.ReadOnly massesInv;
            [ReadOnly]
            private NativeArray<TriangleAreaConstraint> constraints;

            public ApplyAreaConstraintJob(ITriangleAreaConstraint constraint)
            {
                stiffness = constraint.Stiffness;
                positions = constraint.PredictedPositions;
                massesInv = constraint.MassesInv.Value.AsReadOnly();
                constraints = constraint.Constraints.Value.AsDeferredJobArray();
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
                var (pA, pB, pC) = positions.At3(c);
                var (wA, wB, wC) = massesInv.At3(c);

                var pAB = pB - pA;
                var pAC = pC - pA;
                var pBC = pC - pB;

                var lambda = wA * math.lengthsq(pBC) + wB * math.lengthsq(pAC) + wC * math.lengthsq(pAB);
                if (lambda < math.EPSILON)
                {
                    return;
                }
                lambda = stiffness / lambda;

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
            foreach (var constraint in References)
            {
                if (constraint.Stiffness != 0)
                {
                    dependencies = new ApplyAreaConstraintJob(constraint).Schedule(dependencies);
                }
            }

            return dependencies;
        }
    }
}