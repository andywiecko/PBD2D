using andywiecko.ECS;
using andywiecko.PBD2D.Core;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace andywiecko.PBD2D.Systems
{
    public class EdgeMeshTopologySystem : BaseSystem<IEdgeMeshTopology>
    {
        [BurstCompile]
        private struct AdaptTopologyJob : IJob
        {
            private NativeList<Point> points;
            private NativeList<Edge> edges;
            private NativeHashSet<Point> pointsToRemove;
            private NativeHashSet<Point> removedPoints;

            public AdaptTopologyJob(IEdgeMeshTopology component)
            {
                points = component.Points;
                edges = component.Edges;
                pointsToRemove = component.PointsToRemove;
                removedPoints = component.RecentlyRemovedPoints;
            }

            public void Execute()
            {
                if (pointsToRemove.IsEmpty)
                {
                    return;
                }

                removedPoints.Clear();
                foreach (var p in pointsToRemove)
                {
                    var pId = points.IndexOf(p);
                    if (pId >= 0)
                    {
                        points.RemoveAtSwapBack(pId);
                        removedPoints.Add(p);
                    }
                }
                foreach (var p in pointsToRemove)
                {
                    for (int i = edges.Length - 1; i >= 0; i--)
                    {
                        var e = edges[i];
                        if (p.Id == e.IdA)
                        {
                            edges.RemoveAtSwapBack(i);
                            removedPoints.Add(e.IdA);
                        }
                        else if (p.Id == e.IdB)
                        {
                            edges.RemoveAtSwapBack(i);
                            removedPoints.Add(e.IdB);
                        }
                    }
                }

                pointsToRemove.Clear();
            }
        }

        public override JobHandle Schedule(JobHandle dependencies)
        {
            foreach (var c in References)
            {
                dependencies = new AdaptTopologyJob(c).Schedule(dependencies);
            }

            return dependencies;
        }
    }
}