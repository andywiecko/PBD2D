using andywiecko.BurstCollections;
using andywiecko.PBD2D.Core;
using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace andywiecko.PBD2D.Systems
{
    [AddComponentMenu("PBD2D:Systems/Collisions/Friction System")]
    public class FrictionSystem : BaseSystem<IFrictionComponent>
    {
        [SerializeField,Min(0)]
        private float coefficient = 1f;

        [BurstCompile]
        private struct ApplyFrictionJob : IJobParallelFor
        {
            private NativeIndexedArray<Id<Point>, float2>.ReadOnly positions;
            private NativeIndexedArray<Id<Point>, float2> predictedPositions;
            private NativeIndexedArray<Id<Point>, Friction> frictions;
            private readonly float k;
            private readonly float mu;

            public ApplyFrictionJob(IFrictionComponent component, float globalFrictionCoefficient)
            {
                positions = component.Positions;
                predictedPositions = component.PredictedPositions;
                frictions = component.AccumulatedFriction;
                k = 1f; // stiffness todo
                mu = globalFrictionCoefficient;
            }

            public void Execute(int i)
            {
                var pId = (Id<Point>)i;
                var p = positions[pId];
                var q = predictedPositions[pId];
                var dp = q - p;
                var f = frictions[pId];
                if(math.all(dp != 0) && f.Length != 0)
                {
                    var n = f.Normal;
                    var t = dp - math.dot(dp, n) * n;
                    if(math.all(t == 0))
                    {
                        return;
                    }

                    dp = math.min(math.length(t), mu * f.Length) * math.normalizesafe(t);
                    predictedPositions[pId] -= k * dp;
                    frictions[pId] = default;
                }
            }

            public JobHandle Schedule(JobHandle dependencies)
            {
                return this.Schedule(positions.Length, innerloopBatchCount: 64, dependencies);
            }
        }

        public override JobHandle Schedule(JobHandle dependencies)
        {
            foreach(var component in References)
            {
                dependencies = new ApplyFrictionJob(component, coefficient).Schedule(dependencies);
            }

            return dependencies;
        }
    }
}