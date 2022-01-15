using andywiecko.BurstCollections;
using andywiecko.PBD2D.Core;
using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace andywiecko.PBD2D.Systems
{
    [AddComponentMenu("PBD2D:Systems/Position Based Dynamics/Step Start System")]
    public class PositionBasedDynamicsStepStartSystem : BaseSystem<IPositionBasedDynamics>
    {
        [SerializeField, Range(0, 5)]
        private float globalDamping = 0.01f;

        [SerializeField]
        private float2 globalExternalForce = math.float2(x: 0, y: -9.81f * 10);

        [BurstCompile]
        private struct ApplyExternalForcesJob : IJobParallelFor
        {
            private NativeIndexedArray<Id<Point>, float>.ReadOnly massesInv;
            private NativeIndexedArray<Id<Point>, float2> velocities;
            private NativeIndexedArray<Id<Point>, float2> predictedPositions;
            private NativeIndexedArray<Id<Point>, float2>.ReadOnly positions;
            private readonly float deltaTime;
            private readonly float damping;
            private readonly float2 externalForce;

            public ApplyExternalForcesJob(IPositionBasedDynamics component, float damping, float2 externalForce)
            {
                massesInv = component.MassesInv;
                velocities = component.Velocities;
                predictedPositions = component.PredictedPositions;
                positions = component.Positions.Value.AsReadOnly();
                deltaTime = 0.001f;
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

                velocities[pointId] += deltaTime * massInv * (externalForce - damping * velocities[pointId]);
                predictedPositions[pointId] = positions[pointId] + deltaTime * velocities[pointId];
            }
        }

        public override JobHandle Schedule(JobHandle dependencies)
        {
            foreach (var component in References)
            {
                var damping = component.Damping + globalDamping;
                var externalForce = component.ExternalForce + globalExternalForce;
                dependencies = new ApplyExternalForcesJob(component, damping, externalForce).Schedule(component.Positions.Value.Length, innerloopBatchCount: 64, dependencies);
            }

            return dependencies;
        }
    }
}
