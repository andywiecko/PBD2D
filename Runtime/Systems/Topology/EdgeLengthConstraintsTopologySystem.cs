using andywiecko.ECS;
using andywiecko.PBD2D.Core;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace andywiecko.PBD2D.Systems
{
    [Category(PBDCategory.Topology)]
    public class EdgeLengthConstraintsTopologySystem : BaseSystem<IEdgeLengthConstraintsTopology>
    {
        [BurstCompile]
        private struct AdaptConstraintsJob : IJob
        {
            [ReadOnly]
            private NativeHashSet<Point> removedPoints;
            private NativeList<EdgeLengthConstraint> constraints;

            public AdaptConstraintsJob(IEdgeLengthConstraintsTopology c)
            {
                removedPoints = c.RecentlyRemovedPoints;
                constraints = c.Constraints;
            }

            public void Execute()
            {
                foreach (var p in removedPoints)
                {
                    for (int i = constraints.Length - 1; i >= 0; i--)
                    {
                        var c = constraints[i];
                        if (p.Id == c.IdA || p.Id == c.IdB)
                        {
                            constraints.RemoveAtSwapBack(i);
                        }
                    }
                }
            }
        }

        public override JobHandle Schedule(JobHandle dependencies)
        {
            foreach (var c in References)
            {
                dependencies = new AdaptConstraintsJob(c).Schedule(dependencies);
            }

            return dependencies;
        }
    }
}