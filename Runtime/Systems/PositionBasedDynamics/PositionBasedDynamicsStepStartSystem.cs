using andywiecko.BurstCollections;
using andywiecko.ECS;
using andywiecko.PBD2D.Core;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace andywiecko.PBD2D.Systems
{
    [Category(PBDCategory.PBD)]
    public class PositionBasedDynamicsStepStartSystem : BaseSystemWithConfiguration<IPositionBasedDynamics, PBDConfiguration>
    {
        private float GlobalDamping => Configuration.GlobalDamping;
        private float2 GlobalExternalAcceleration => Configuration.GlobalExternalAcceleration;

        [BurstCompile]
        private struct SolveStepStartJob : IJobParallelForDefer
        {
            [ReadOnly]
            private NativeArray<Point> points;
            private NativeIndexedArray<Id<Point>, float2> velocities;
            private NativeIndexedArray<Id<Point>, float2> positions;
            private NativeIndexedArray<Id<Point>, float2> previousPositions;
            private readonly float dt;
            private readonly float gamma;
            private readonly float2 a;

            public SolveStepStartJob(IPositionBasedDynamics component, float damping, float2 acceleration, float deltaTime)
            {
                points = component.Points.Value.AsDeferredJobArray();
                velocities = component.Velocities;
                positions = component.Positions;
                previousPositions = component.PreviousPositions;
                dt = deltaTime;
                gamma = damping;
                a = acceleration;
            }

            public void Execute(int index)
            {
                var pointId = points[index].Id;
                var v = velocities[pointId];
                var p = positions[pointId];
                v += (a - gamma * v) * dt;
                previousPositions[pointId] = p;
                positions[pointId] += v * dt;
                velocities[pointId] = v;
            }
        }

        public override JobHandle Schedule(JobHandle dependencies = default)
        {
            foreach (var component in References)
            {
                var acceleration = component.ExternalAcceleration + GlobalExternalAcceleration;
                var damping = component.Damping + GlobalDamping;
                dependencies = new SolveStepStartJob(component, damping, acceleration, Configuration.ReducedDeltaTime)
                    .Schedule(component.Points.Value, innerloopBatchCount: 64, dependencies);
            }

            return dependencies;
        }
    }
}
