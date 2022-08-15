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
    public class EdgeLengthConstraintsSystem : BaseSystemWithConfiguration<IEdgeLengthConstraints, PBDConfiguration>
    {
        [BurstCompile]
        private struct ApplyEdgeConstraintJob : IJob
        {
            private NativeIndexedArray<Id<Point>, float2> positions;
            private NativeIndexedArray<Id<Point>, float>.ReadOnly weights;
            [ReadOnly]
            private NativeArray<EdgeLengthConstraint> constraints;
            private readonly float k;
            private readonly float a;

            public ApplyEdgeConstraintJob(IEdgeLengthConstraints component, float dt)
            {
                positions = component.Positions;
                weights = component.Weights.Value.AsReadOnly();
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

                var (wA, wB) = weights.At(c);
                var wAB = wA + wB + a;
                if (wAB <= math.EPSILON)
                {
                    return;
                }

                var (pA, pB) = positions.At(c);
                var pAB = pB - pA;
                var pABabs = math.length(pAB);

                if (pABabs <= math.EPSILON)
                {
                    return;
                }

                var n = -pAB / pABabs;
                var C = pABabs - l;
                var lambda = -C / wAB;
                var dp = k * lambda * n;
                positions[idA] += wA * dp;
                positions[idB] -= wB * dp;
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
