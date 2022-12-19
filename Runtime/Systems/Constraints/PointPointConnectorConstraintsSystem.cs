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
    public class PointPointConnectorConstraintsSystem : BaseSystemWithConfiguration<IPointPointConnectorConstraints, PBDConfiguration>
    {
        [BurstCompile]
        private struct ApplyConstraintJob : IJob
        {
            private readonly float k;
            private readonly float a;
            private readonly float w0;
            [ReadOnly]
            private NativeArray<PointPointConnectorConstraint> constraints;
            private NativeIndexedArray<Id<Point>, float2> connecteePositions;
            private NativeIndexedArray<Id<Point>, float2> connecterPositions;
            private NativeIndexedArray<Id<Point>, float>.ReadOnly connecteeWeights;
            private NativeIndexedArray<Id<Point>, float>.ReadOnly connecterWeights;

            public ApplyConstraintJob(IPointPointConnectorConstraints component, float dt)
            {
                k = component.Stiffness;
                a = component.Compliance / dt / dt;
                w0 = component.Weight;
                constraints = component.Constraints.Value.AsDeferredJobArray();
                connecteePositions = component.Connectee.Positions.Value;
                connecterPositions = component.Connecter.Positions.Value;
                connecteeWeights = component.Connectee.Weights.Value.AsReadOnly();
                connecterWeights = component.Connecter.Weights.Value.AsReadOnly();
            }

            public void Execute()
            {
                foreach (var c in constraints)
                {
                    Execute(c);
                }
            }

            private void Execute(PointPointConnectorConstraint c)
            {
                var (idP, idQ) = c;
                var (p, q) = (connecteePositions[idP], connecterPositions[idQ]);
                var (wp, wq) = (connecteeWeights[idP], connecterWeights[idQ]);
                var (wpt, wqt) = (w0 * wp, (1 - w0) * wq);

                var w = wpt + wqt;
                if (w == 0)
                {
                    return;
                }

                var dx = -k / (w + 0.5f * a) * (p - q);
                connecteePositions[idP] += wpt * dx;
                connecterPositions[idQ] -= wqt * dx;
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