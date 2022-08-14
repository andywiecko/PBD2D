using andywiecko.BurstCollections;
using andywiecko.ECS;
using Unity.Collections;
using Unity.Mathematics;

namespace andywiecko.PBD2D.Core
{
    public interface IPositionBasedDynamics : IComponent
    {
        Ref<NativeList<Point>> Points { get; }
        Ref<NativeIndexedArray<Id<Point>, float2>> PreviousPositions { get; }
        Ref<NativeIndexedArray<Id<Point>, float2>> Positions { get; }
        Ref<NativeIndexedArray<Id<Point>, float2>> Velocities { get; }
        float2 ExternalAcceleration { get; }
        float Damping { get; }
    }

    public interface IPositionBasedFluid : IComponent
    {
        Ref<NativeIndexedArray<Id<Point>, float2>> PreviousPositions { get; }
        Ref<NativeIndexedArray<Id<Point>, float2>> Positions { get; }
        Ref<NativeIndexedArray<Id<Point>, float2>> Velocities { get; }
        Ref<NativeIndexedArray<Id<Point>, int2>> PointsGridCell { get; }
        Ref<NativeIndexedArray<Id<Point>, FixedList128Bytes<int2>>> PointsNeighborCells { get; }
        Ref<NativeIndexedArray<Id<Point>, FixedList4096Bytes<Id<Point>>>> PointsNeighbors { get; }
        Ref<NativeHashSet<int2>> OccupiedGridCells { get; }
        Ref<NativeHashMap<int2, FixedList4096Bytes<Id<Point>>>> Grid { get; }
        Ref<NativeIndexedArray<Id<Point>, float>> Lambdas { get; }
        Ref<NativeIndexedArray<Id<Point>, float2>> DeltaPositions { get; }
        float2 ExternalForce { get; }
        int2 GridSize { get; }
        float CellSize { get; }
        float Width { get; }
        float Height { get; }
        float InteractionRadius { get; }
        float RestDensity { get; }
    }
}
