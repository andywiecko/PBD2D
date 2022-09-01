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
}
