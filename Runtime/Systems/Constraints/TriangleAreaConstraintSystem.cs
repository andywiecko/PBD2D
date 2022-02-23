using andywiecko.BurstCollections;
using andywiecko.BurstMathUtils;
using andywiecko.PBD2D.Core;
using Unity.Burst;
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
            private NativeIndexedArray<Id<Triangle>, Triangle>.ReadOnly triangles;
            private NativeIndexedArray<Id<Triangle>, float>.ReadOnly restAreas2;

            public ApplyAreaConstraintJob(ITriangleAreaConstraint constraint)
            {
                stiffness = constraint.Stiffness;
                positions = constraint.PredictedPositions;
                massesInv = constraint.MassesInv.Value.AsReadOnly();
                triangles = constraint.Triangles.Value.AsReadOnly();
                restAreas2 = constraint.RestAreas2.Value.AsReadOnly();
            }

            public void Execute()
            {
                foreach (var i in 0..triangles.Length)
                {
                    ApplyAreaConstraint((Id<Triangle>)i);
                }
            }

            private void ApplyAreaConstraint(Id<Triangle> triId)
            {
                var triangle = triangles[triId];
                var (idA, idB, idC) = triangle;
                var (pA, pB, pC) = positions.At(triangle);
                var (mInvA, mInvB, mInvC) = massesInv.At(triangle);

                var pAB = pB - pA;
                var pAC = pC - pA;
                var pBC = pC - pB;

                var lambda = mInvA * math.lengthsq(pBC) + mInvB * math.lengthsq(pAC) + mInvC * math.lengthsq(pAB);
                if (lambda < Constants.FloatEpsilon)
                {
                    return;
                }
                lambda = stiffness / lambda;

                var A = MathUtils.Cross(pAB, pAC);
                if (math.abs(A) < Constants.FloatEpsilon)
                {
                    return;
                }

                var C = A - restAreas2[triId];
                positions[idA] += lambda * mInvA * C * MathUtils.Rotate90CW(pBC);
                positions[idB] += lambda * mInvB * C * MathUtils.Rotate90CW(-pAC);
                positions[idC] += lambda * mInvC * C * MathUtils.Rotate90CW(pAB);
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