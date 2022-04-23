using andywiecko.BurstCollections;
using andywiecko.PBD2D.Core;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace andywiecko.PBD2D.Systems
{
    [AddComponentMenu("PBD2D:Systems/Constraints/Edge Length Constraints System")]
    public class EdgeLengthConstraintsSystem : BaseSystem<IEdgeLengthConstraints>
    {
        [BurstCompile]
        private struct ApplyEdgeConstraintJob : IJob
        {
            private NativeIndexedArray<Id<Point>, float2> predictedPositions;
            private NativeIndexedArray<Id<Point>, float>.ReadOnly massesInv;
            [ReadOnly]
            private NativeArray<EdgeLengthConstraint> constraints;
            private readonly float stiffness;

            public ApplyEdgeConstraintJob(IEdgeLengthConstraints component)
            {
                predictedPositions = component.PredictedPositions;
                massesInv = component.MassesInv.Value.AsReadOnly();
                constraints = component.Constraints.Value.AsDeferredJobArray();
                stiffness = component.Stiffness;
            }

            public void Execute()
            {
                foreach (var c in constraints)
                {
                    Execute(c);
                }
            }

            private void Execute(EdgeLengthConstraint c)
            {
                var (idA, idB, restLength) = c;

                var (wA, wB) = massesInv.At2(c);
                var wAB = wA + wB;
                if (wAB <= math.EPSILON)
                {
                    return;
                }

                var (pA, pB) = predictedPositions.At2(c);
                var pAB = pB - pA;
                var length = math.length(pAB);
                if (length < math.EPSILON)
                {
                    return;
                }

                var dP = stiffness * (1f / wAB) * (length - restLength) * pAB / length;

                predictedPositions[idA] += wA * dP;
                predictedPositions[idB] -= wB * dP;
            }
        }

        public override JobHandle Schedule(JobHandle dependencies = default)
        {
            foreach (var component in References)
            {
                if (component.Stiffness != 0)
                {
                    dependencies = new ApplyEdgeConstraintJob(component).Schedule(dependencies);
                }
            }

            return dependencies;
        }
    }
}
