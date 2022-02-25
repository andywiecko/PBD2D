using andywiecko.BurstCollections;
using andywiecko.BurstMathUtils;
using Unity.Collections;
using Unity.Mathematics;

namespace andywiecko.PBD2D.Core
{
    public interface IEdgeLengthConstraint : IComponent
    {
        float Stiffness { get; }
        Ref<NativeIndexedArray<Id<Point>, float2>> PredictedPositions { get; }
        Ref<NativeIndexedArray<Id<Point>, float>> MassesInv { get; }
        Ref<NativeIndexedArray<Id<Edge>, Edge>> Edges { get; }
        Ref<NativeIndexedArray<Id<Edge>, float>> RestLengths { get; }
    }

    public interface IEdgeBendingConstraint : IComponent
    {
        float Stiffness { get; }
        Ref<NativeIndexedArray<Id<Point>, float2>> PredictedPositions { get; }
        NativeIndexedArray<Id<Point>, float>.ReadOnly MassesInv { get; }
        NativeIndexedArray<Id<Frame>, Frame>.ReadOnly Frames { get; }
        NativeIndexedArray<Id<Frame>, float>.ReadOnly RestAngles { get; }
    }

    public interface IAttachmentConstraintRod : IComponent
    {
        bool LockOrientation { get; }
        bool IsDirectedForward { get; }
        Id<Edge> EdgeId { get; }
        float2 AttachmentPosition { get; }
        float2 AttachmentDirection { get; }
        Ref<NativeIndexedArray<Id<Point>, float2>> PredictedPositions { get; }
        NativeIndexedArray<Id<Edge>, Edge>.ReadOnly Edges { get; }
    }

    public interface IPointEdgeDistanceConstraint : IComponent
    {
        float Stiffness { get; }
        float Weight { get; }
        Id<Point> ConnecteePointId { get; }
        Id<Edge> ConnecterEdgeId { get; }
        Ref<NativeReference<float>> RestLength { get; }
        Ref<NativeIndexedArray<Id<Point>, float2>> ConnecterPredictedPositions { get; }
        Ref<NativeIndexedArray<Id<Point>, float2>> ConnecteePredictedPositions { get; }
        NativeIndexedArray<Id<Point>, float>.ReadOnly ConnecterMassesInv { get; }
        NativeIndexedArray<Id<Point>, float>.ReadOnly ConnecteeMassesInv { get; }
        NativeIndexedArray<Id<Edge>, Edge>.ReadOnly ConnecterEdges { get; }
    }

    public interface IEdgeBendingConstraintRodConnector : IComponent
    {
        float Stiffness { get; }
        float Weight { get; }
        float RestAngle { get; }
        Id<Edge> ConnecterEdgeId { get; }
        Id<Edge> ConnecteeEdgeId { get; }
        Ref<NativeIndexedArray<Id<Point>, float2>> ConnecterPredictedPositions { get; }
        Ref<NativeIndexedArray<Id<Point>, float2>> ConnecteePredictedPositions { get; }
        NativeIndexedArray<Id<Point>, float>.ReadOnly ConnecterMassesInv { get; }
        NativeIndexedArray<Id<Point>, float>.ReadOnly ConnecteeMassesInv { get; }
        NativeIndexedArray<Id<Edge>, Edge>.ReadOnly ConnecterEdges { get; }
        NativeIndexedArray<Id<Edge>, Edge>.ReadOnly ConnecteeEdges { get; }
    }

    public interface ITriangleAreaConstraint : IComponent
    {
        float Stiffness { get; }
        Ref<NativeIndexedArray<Id<Point>, float2>> PredictedPositions { get; }
        Ref<NativeIndexedArray<Id<Point>, float>> MassesInv { get; }
        Ref<NativeIndexedArray<Id<Triangle>, Triangle>> Triangles { get; }
        Ref<NativeIndexedArray<Id<Triangle>, float>> RestAreas2 { get; }
    }

    public interface IShapeMatchingConstraint : IComponent
    {
        float Stiffness { get; }
        float Beta { get; }
        float TotalMass { get; }
        Ref<NativeIndexedArray<Id<Point>, float>> MassesInv { get; }
        Ref<NativeIndexedArray<Id<Point>, float2>> InitialRelativePositions { get; }
        Ref<NativeIndexedArray<Id<Point>, float2>> PredictedPositions { get; }
        Ref<NativeIndexedArray<Id<Point>, float2>> RelativePositions { get; }
        Ref<NativeReference<float2>> CenterOfMass { get; }
        Ref<NativeReference<float2x2>> ApqMatrix { get; }
        float2x2 AqqMatrix { get; }
        Ref<NativeReference<float2x2>> AMatrix { get; }
        Ref<NativeReference<Complex>> Rotation { get; }
    }

    public interface IContactConstraintRod : IComponent
    {
        float Stiffness { get; }
        float ContactRadius { get; }
        NativeIndexedArray<Id<Edge>, Edge>.ReadOnly Edges { get; }
        NativeIndexedArray<Id<Point>, float>.ReadOnly MassesInv { get; }
        Ref<NativeIndexedArray<Id<Point>, float2>> PredictedPositions { get; }
        Ref<NativeIndexedArray<Id<Edge>, AABB>> AABBs { get; }
        Ref<NativeIndexedList<Id<PotentialCollision>, IdPair<Edge>>> PotentialCollisions { get; }
        Ref<NativeIndexedList<Id<ContactInfo>, ContactInfo>> ContactInfo { get; }
    }
}
