using Unity.Burst;
using Unity.Entities;

namespace andywiecko.PBD2D
{
    [UpdateInGroup(typeof(PBDEndSystemGroup))]
    public partial struct PBDEndSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            new Job().ScheduleParallel();
        }

        [BurstCompile]
        private partial struct Job : IJobEntity
        {
            public void Execute(ref Velocity velocity, in Position position, in PreviousPosition previousPosition)
            {
                var dt = 0.01f;

                var p = position.Value;
                var q = previousPosition.Value;
                velocity.Value = (p - q) / dt;
            }
        }
    }
}