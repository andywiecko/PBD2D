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
    public class EdgeLengthConstraintsSystem : BaseSystemWithConfiguration<IEdgeLengthConstraints, SimulationConfiguration>
    {
        [BurstCompile]
        private struct ApplyEdgeConstraintJob : IJob
        {
            private NativeIndexedArray<Id<Point>, float2> predictedPositions;
            private NativeIndexedArray<Id<Point>, float>.ReadOnly massesInv;
            [ReadOnly]
            private NativeArray<EdgeLengthConstraint> constraints;
            private readonly float k;
            private readonly float a;

            public ApplyEdgeConstraintJob(IEdgeLengthConstraints component, float dt)
            {
                predictedPositions = component.PredictedPositions;
                massesInv = component.MassesInv.Value.AsReadOnly();
                constraints = component.Constraints.Value.AsDeferredJobArray();
                k = component.Stiffness;
                a = component.Compliance / dt / dt;
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
                var (idA, idB, l) = c;

                var (wA, wB) = massesInv.At(c);
                var wAB = wA + wB;
                if (wAB <= math.EPSILON)
                {
                    return;
                }

                var (pA, pB) = predictedPositions.At(c);
                var pAB = pB - pA;
                var pABabs = math.length(pAB);

                if (pABabs < math.EPSILON)
                {
                    return;
                }

                var n = -pAB / pABabs;
                var C = pABabs - l;
                var lambda = -C / (wAB + a);
                var dp = k * lambda * n;
                predictedPositions[idA] += wA * dp;
                predictedPositions[idB] -= wB * dp;
            }
        }

        public override JobHandle Schedule(JobHandle dependencies = default)
        {
            foreach (var component in References)
            {
                if (component.Stiffness != 0)
                {
                    dependencies = new ApplyEdgeConstraintJob(component, Configuration.ReducedDeltaTime).Schedule(dependencies);
                }
            }

            return dependencies;
        }
    }
}
