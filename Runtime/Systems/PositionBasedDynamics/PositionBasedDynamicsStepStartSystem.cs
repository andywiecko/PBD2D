using andywiecko.BurstCollections;
using andywiecko.ECS;
using andywiecko.PBD2D.Core;
using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;

namespace andywiecko.PBD2D.Systems
{
    [Category("Position Based Dynamics")]
    public class PositionBasedDynamicsStepStartSystem : BaseSystemWithConfiguration<IPositionBasedDynamics, PBDConfiguration>
    {
        private float GlobalDamping => Configuration.GlobalDamping;
        private float2 GlobalExternalForce => Configuration.GlobalExternalForce;

        [BurstCompile]
        private struct ApplyExternalForcesJob : IJobParallelFor
        {
            private NativeIndexedArray<Id<Point>, float>.ReadOnly massesInv;
            private NativeIndexedArray<Id<Point>, float2> velocities;
            private NativeIndexedArray<Id<Point>, float2> predictedPositions;
            private NativeIndexedArray<Id<Point>, float2>.ReadOnly positions;
            private readonly float dt;
            private readonly float damping;
            private readonly float2 externalForce;

            public ApplyExternalForcesJob(IPositionBasedDynamics component, float damping, float2 externalForce, float dt)
            {
                massesInv = component.MassesInv.Value.AsReadOnly();
                velocities = component.Velocities;
                predictedPositions = component.PredictedPositions;
                positions = component.Positions.Value.AsReadOnly();
                this.dt = dt;
                this.damping = damping;
                this.externalForce = externalForce;
            }

            public void Execute(int index)
            {
                var pointId = (Id<Point>)index;
                var massInv = massesInv[pointId];
                if (massInv == 0f)
                {
                    return;
                }

                velocities[pointId] += dt * massInv * (externalForce - damping * velocities[pointId]);
                predictedPositions[pointId] = positions[pointId] + dt * velocities[pointId];
            }
        }

        public override JobHandle Schedule(JobHandle dependencies = default)
        {
            foreach (var component in References)
            {
                var damping = component.Damping + GlobalDamping;
                var externalForce = component.ExternalForce + GlobalExternalForce;
                dependencies = new ApplyExternalForcesJob(component, damping, externalForce, dt: Configuration.ReducedDeltaTime).Schedule(component.Positions.Value.Length, innerloopBatchCount: 64, dependencies);
            }

            return dependencies;
        }
    }
}
