using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

namespace andywiecko.PBD2D
{
    [UpdateInGroup(typeof(PBDStartSystemGroup))]
    public partial struct PBDStartSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            new Job().ScheduleParallel();
        }

        [BurstCompile]
        private partial struct Job : IJobEntity
        {
            public void Execute(ref Velocity velocity, ref Position position, ref PreviousPosition previousPosition)
            {
                var a = math.float2(0, -10);
                var gamma = 0;
                var dt = 0.01f;

                var v = velocity.Value;
                var p = position.Value;
                v += (a - gamma * v) * dt;

                previousPosition.Value = p;
                p += v * dt;
                position.Value = p;
                velocity.Value = v;
            }
        }
    }
}