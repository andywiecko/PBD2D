using andywiecko.BurstCollections;
using andywiecko.ECS;
using Unity.Collections;
using Unity.Mathematics;

namespace andywiecko.PBD2D.Core
{
    public interface IMouseInteractionComponent : IComponent
    {
        float Stiffness { get; }
        Ref<NativeIndexedArray<Id<Point>, float2>> Positions { get; }
        Ref<NativeList<MouseInteractionConstraint>> Constraints { get; }
    }
}
