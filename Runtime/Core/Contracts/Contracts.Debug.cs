using andywiecko.BurstCollections;
using Unity.Collections;
using Unity.Mathematics;

namespace andywiecko.PBD2D.Core
{
    public interface IMouseInteractionComponent : IComponent
    {
        Ref<NativeIndexedArray<Id<Point>, float2>> PredictedPositions { get; }
        Ref<NativeReference<Id<Point>>> InteractingPointId { get; }
        Ref<NativeReference<float2>> Offset { get; }
    }
}
