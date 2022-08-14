using andywiecko.BurstCollections;
using andywiecko.ECS;
using Unity.Collections;
using Unity.Mathematics;

namespace andywiecko.PBD2D.Core
{
    public interface IMouseInteractionComponent : IComponent
    {
        Ref<NativeIndexedArray<Id<Point>, float2>> Positions { get; }
        Ref<NativeReference<Id<Point>>> InteractingPointId { get; }
        Ref<NativeReference<float2>> Offset { get; }
    }
}
