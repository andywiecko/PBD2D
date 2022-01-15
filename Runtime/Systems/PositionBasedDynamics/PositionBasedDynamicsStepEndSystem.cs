using andywiecko.BurstCollections;
using andywiecko.PBD2D.Core;
using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace andywiecko.PBD2D.Systems
{
    [AddComponentMenu("PBD2D:Systems/Position Based Dynamics/Step End System")]
    public class PositionBasedDynamicsStepEndSystem : BaseSystem<IPositionBasedDynamics>
    {
        [BurstCompile]
        private struct MovePositionsAndUpdateVelocityJob : IJobParallelFor
        {
            private NativeIndexedArray<Id<Point>, float2> velocities;
            private NativeIndexedArray<Id<Point>, float2> positions;
            private NativeIndexedArray<Id<Point>, float2>.ReadOnly predictedPositions;
            private readonly float deltaTime;
            private readonly NativeIndexedArray<Id<Point>, float>.ReadOnly massesInv;

            public MovePositionsAndUpdateVelocityJob(IPositionBasedDynamics component)
            {
                velocities = component.Velocities;
                positions = component.Positions;
                predictedPositions = component.PredictedPositions.Value.AsReadOnly();
                deltaTime = 0.001f;
                massesInv = component.MassesInv;
            }

            public void Execute(int index)
            {
                var pointId = (Id<Point>)index;
                if (massesInv[pointId] != 0)
                {
                    var predictedPosition = predictedPositions[pointId];
                    velocities[pointId] = (predictedPosition - positions[pointId]) / deltaTime;
                    positions[pointId] = predictedPosition;
                }
            }
        }

        public override JobHandle Schedule(JobHandle dependencies)
        {
            foreach (var component in References)
            {
                dependencies = new MovePositionsAndUpdateVelocityJob(component).Schedule(component.Positions.Value.Length, 64, dependencies);
            }

            return dependencies;
        }
    }
}
