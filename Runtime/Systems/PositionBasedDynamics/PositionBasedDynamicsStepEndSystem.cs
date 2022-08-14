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
    public class PositionBasedDynamicsStepEndSystem : BaseSystemWithConfiguration<IPositionBasedDynamics, PBDConfiguration>
    {
        [BurstCompile]
        private struct SolveStepEndJob : IJobParallelForDefer
        {
            [ReadOnly]
            private NativeArray<Point> points;
            private NativeIndexedArray<Id<Point>, float2> velocities;
            private NativeIndexedArray<Id<Point>, float2>.ReadOnly previousPositions;
            private NativeIndexedArray<Id<Point>, float2>.ReadOnly positions;
            private readonly float dtInv;

            public SolveStepEndJob(IPositionBasedDynamics component, float dt)
            {
                points = component.Points.Value.AsDeferredJobArray();
                velocities = component.Velocities;
                previousPositions = component.PreviousPositions.Value.AsReadOnly();
                positions = component.Positions.Value.AsReadOnly();
                dtInv = 1f / dt;
            }

            public void Execute(int index)
            {
                var pointId = points[index].Id;
                var p = positions[pointId];
                var q = previousPositions[pointId];
                var v = (p - q) * dtInv;
                velocities[pointId] = v;
            }
        }

        public override JobHandle Schedule(JobHandle dependencies = default)
        {
            foreach (var component in References)
            {
                dependencies = new SolveStepEndJob(component, dt: Configuration.ReducedDeltaTime)
                    .Schedule(component.Points.Value, 64, dependencies);
            }

            return dependencies;
        }
    }
}
