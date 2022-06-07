using andywiecko.BurstCollections;
using andywiecko.ECS;
using andywiecko.PBD2D.Core;
using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace andywiecko.PBD2D.Systems
{
    [AddComponentMenu("PBD2D:Systems/Position Based Dynamics/Step End System")]
    public class PositionBasedDynamicsStepEndSystem : BaseSystemWithConfiguration<IPositionBasedDynamics, SimulationConfiguration>
    {
        [BurstCompile]
        private struct MovePositionsAndUpdateVelocityJob : IJobParallelFor
        {
            private NativeIndexedArray<Id<Point>, float2> velocities;
            private NativeIndexedArray<Id<Point>, float2> positions;
            private NativeIndexedArray<Id<Point>, float2>.ReadOnly predictedPositions;
            private readonly float dt;
            private NativeIndexedArray<Id<Point>, float>.ReadOnly massesInv;

            public MovePositionsAndUpdateVelocityJob(IPositionBasedDynamics component, float dt)
            {
                velocities = component.Velocities;
                positions = component.Positions;
                predictedPositions = component.PredictedPositions.Value.AsReadOnly();
                this.dt = dt;
                massesInv = component.MassesInv.Value.AsReadOnly();
            }

            public void Execute(int index)
            {
                var pointId = (Id<Point>)index;
                if (massesInv[pointId] != 0)
                {
                    var predictedPosition = predictedPositions[pointId];
                    velocities[pointId] = (predictedPosition - positions[pointId]) / dt;
                    positions[pointId] = predictedPosition;
                }
            }
        }

        public override JobHandle Schedule(JobHandle dependencies = default)
        {
            foreach (var component in References)
            {
                dependencies = new MovePositionsAndUpdateVelocityJob(component, dt: Configuration.ReducedDeltaTime)
                    .Schedule(component.Positions.Value.Length, 64, dependencies);
            }

            return dependencies;
        }
    }
}
