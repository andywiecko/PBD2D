using andywiecko.BurstCollections;
using andywiecko.BurstMathUtils;
using andywiecko.ECS;
using Unity.Collections;
using Unity.Mathematics;

namespace andywiecko.PBD2D.Core
{
    public interface IPositionConstraints : IComponent
    {
        float Stiffness { get; }
        float Compliance { get; }
        Ref<NativeIndexedArray<Id<Point>, float2>> Positions { get; }
        Ref<NativeIndexedArray<Id<Point>, float>> Weights { get; }
        Ref<NativeList<PositionConstraint>> Constraints { get; }
    }

    public interface IEdgeLengthConstraints : IComponent
    {
        float Stiffness { get; }
        float Compliance { get; }
        Ref<NativeIndexedArray<Id<Point>, float2>> Positions { get; }
        Ref<NativeIndexedArray<Id<Point>, float>> Weights { get; }
        Ref<NativeList<EdgeLengthConstraint>> Constraints { get; }
    }

    public interface ITriangleAreaConstraints : IComponent
    {
        float Stiffness { get; }
        float Compliance { get; }
        Ref<NativeIndexedArray<Id<Point>, float2>> Positions { get; }
        Ref<NativeIndexedArray<Id<Point>, float>> Weights { get; }
        Ref<NativeList<TriangleAreaConstraint>> Constraints { get; }
    }

    public interface IShapeMatchingConstraint : IComponent
    {
        float Stiffness { get; }
        float Beta { get; }
        float TotalMass { get; }
        Ref<NativeIndexedArray<Id<Point>, float>> Weights { get; }
        Ref<NativeIndexedArray<Id<Point>, float2>> Positions { get; }
        Ref<NativeReference<float2>> CenterOfMass { get; }
        Ref<NativeReference<float2x2>> ApqMatrix { get; }
        Ref<NativeReference<float2x2>> AqqMatrix { get; }
        Ref<NativeReference<float2x2>> AMatrix { get; }
        Ref<NativeReference<Complex>> Rotation { get; }
        Ref<NativeList<PointShapeMatchingConstraint>> Constraints { get; }
    }
}
