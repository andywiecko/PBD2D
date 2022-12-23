using andywiecko.BurstCollections;
using andywiecko.ECS;
using Unity.Collections;

namespace andywiecko.PBD2D.Core
{
    public interface IEdgeMeshTopology : IComponent
    {
        Ref<NativeList<Point>> Points { get; }
        Ref<NativeList<Edge>> Edges { get; }
        Ref<NativeHashSet<Point>> PointsToRemove { get; }
        Ref<NativeHashSet<Point>> RecentlyRemovedPoints { get; }
    }

    public interface IEdgeLengthConstraintsTopology : IComponent
    {
        Ref<NativeList<EdgeLengthConstraint>> Constraints { get; }
        Ref<NativeHashSet<Point>> RecentlyRemovedPoints { get; }
    }
}
