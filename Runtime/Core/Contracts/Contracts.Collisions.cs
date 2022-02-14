using andywiecko.BurstCollections;
using Unity.Collections;
using Unity.Mathematics;

namespace andywiecko.PBD2D.Core
{
    public interface ITriMeshCollideWithFluid : IComponent
    {
        float Weight { get; }
        bool IsValid { get; } //HACK
        Ref<NativeIndexedArray<Id<Point>, float2>> PredictedPositions { get; }
        NativeIndexedArray<Id<Triangle>, Id<ExternalEdge>>.ReadOnly TrianglesEdgesField { get; }
        NativeIndexedArray<Id<Triangle>, AABB>.ReadOnly AABBs { get; }
        NativeIndexedArray<Id<Triangle>, Triangle>.ReadOnly Triangles { get; }
        NativeIndexedArray<Id<ExternalEdge>, ExternalEdge>.ReadOnly ExternalEdges { get; }
        NativeIndexedArray<Id<Point>, float>.ReadOnly MassesInv { get; }
    }
    public interface IFluidCollideWithTriMesh : IComponent
    {
        float ParticleRadius { get; }
        float2 CellSize { get; }
        Ref<NativeIndexedArray<Id<Point>, float2>> PredictedPositions { get; }
        Ref<NativeHashSet<int2>> OccupiedGridCells { get; }
        Ref<NativeHashMap<int2, FixedList4096Bytes<Id<Point>>>> Grid { get; }
    }
    public interface ITriMeshFluidCollisionTuple : IComponent
    {
        float Weight { get; }
        bool IsValid { get; } // HACK: get rid of this shiet
        Ref<NativeList<PointTrianglePair>> PotentialCollisions { get; }
        Ref<NativeList<PointTrianglePair>> Collisions { get; }
        Ref<NativeList<int2>> GridCellsToCheck { get; }

        Ref<NativeIndexedArray<Id<Point>, float2>> TriMeshPredictedPositions { get; }
        NativeIndexedArray<Id<Triangle>, Id<ExternalEdge>>.ReadOnly TrianglesEdgesField { get; }
        NativeIndexedArray<Id<Triangle>, AABB>.ReadOnly AABBs { get; }
        NativeIndexedArray<Id<Triangle>, Triangle>.ReadOnly Triangles { get; }
        NativeIndexedArray<Id<ExternalEdge>, ExternalEdge>.ReadOnly ExternalEdges { get; }
        NativeIndexedArray<Id<Point>, float>.ReadOnly TriMeshMassesInv { get; }

        float ParticleRadius { get; }
        float2 CellSize { get; }
        Ref<NativeIndexedArray<Id<Point>, float2>> FluidPredictedPositions { get; }
        Ref<NativeHashSet<int2>> OccupiedGridCells { get; }
        Ref<NativeHashMap<int2, FixedList4096Bytes<Id<Point>>>> Grid { get; }
    }
    public interface ITrianglesColliderTriMesh : IComponent
    {
        float Margin { get; }
        Ref<NativeIndexedArray<Id<Triangle>, AABB>> AABBs { get; }
        NativeIndexedArray<Id<Point>, float2>.ReadOnly PredictedPositions { get; }
        NativeIndexedArray<Id<Triangle>, Triangle>.ReadOnly Triangles { get; }
    }
    public interface IExternalEdgesTriMesh : IComponent
    {
        Ref<NativeIndexedArray<Id<ExternalEdge>, ExternalEdge>> ExternalEdges { get; }
    }
    public interface IExternalEdgesColliderTriMesh : IComponent
    {
        float CollisionRadius { get; }
        float Margin { get; }
        Ref<NativeIndexedArray<Id<ExternalEdge>, AABB>> AABBs { get; }
        NativeIndexedArray<Id<ExternalEdge>, ExternalEdge>.ReadOnly ExternalEdges { get; }
        NativeIndexedArray<Id<Edge>, Edge>.ReadOnly Edges { get; }
        NativeIndexedArray<Id<Point>, float2>.ReadOnly PredictedPositions { get; }

    }
    public interface IFlowFieldTriMesh : IComponent
    {
        Ref<NativeIndexedArray<Id<Triangle>, Id<ExternalEdge>>> TrianglesEdgesField { get; }
    }
    public interface ITriMeshCollideWithTriMesh : IComponent
    {
        float CollisionRadius { get; }
        Ref<NativeIndexedArray<Id<Point>, float2>> PredictedPositions { get; }
        NativeIndexedArray<Id<Point>, float>.ReadOnly MassesInv { get; }
        NativeIndexedArray<Id<ExternalEdge>, AABB>.ReadOnly AABBs { get; }
        NativeIndexedArray<Id<Edge>, Edge>.ReadOnly Edges { get; }
        NativeIndexedArray<Id<ExternalEdge>, ExternalEdge>.ReadOnly ExternalEdges { get; }
    }

    public interface ITriMeshTriMeshCollisionTuple : IComponent
    {
        Ref<NativeIndexedList<Id<Contact>, ExternalEdgePair>> PotentialCollisions { get; }
        Ref<NativeIndexedList<Id<Contact>, ExternalEdgesContactInfo>> Collisions { get; }
        void Deconstruct(out ITriMeshCollideWithTriMesh triMesh1, out ITriMeshCollideWithTriMesh trimesh2);
    }

    public interface ITriangleBoundingVolumeTreeTriMesh : IComponent
    {
        float Margin { get; }
        Ref<BoundingVolumeTree<AABB>> Tree { get; }
        Ref<NativeIndexedArray<Id<Triangle>, AABB>> AABBs { get; }
        NativeIndexedArray<Id<Point>, float2>.ReadOnly Positions { get; }
        NativeIndexedArray<Id<Triangle>, Triangle>.ReadOnly Triangles { get; }
    }

    #region Capsule-capsule collisions
    public interface ICapsuleCollideWithCapsule : IComponent
    {
        float CollisionRadius { get; }
        Ref<NativeIndexedArray<Id<Point>, float2>> PredictedPositions { get; }
        NativeIndexedArray<Id<Point>, float>.ReadOnly MassesInv { get; }
        NativeIndexedArray<Id<Edge>, Edge>.ReadOnly Edges { get; }
        NativeIndexedArray<Id<CollidableEdge>, Id<Edge>>.ReadOnly CollidableEdges { get; }
        NativeIndexedArray<Id<CollidableEdge>, AABB>.ReadOnly AABBs { get; }
        Ref<NativeIndexedArray<Id<Point>, Friction>> AccumulatedFriction { get; }
    }

    public interface ITriMeshCapsulesCollideWithTriMeshCapsules : ICapsuleCollideWithCapsule { }
    public interface IRodCapsulesCollideWithTriMeshCapsules : ICapsuleCollideWithCapsule { }
    public interface ITriMeshCapsulesCollideWithRodCapsules : ICapsuleCollideWithCapsule { }

    public interface ICapsuleCapsuleCollisionTuple : IComponent
    {
        Ref<NativeList<EdgePair>> PotentialCollisions { get; }
        Ref<NativeList<EdgeEdgeContactInfo>> Collisions { get; }
        ICapsuleCollideWithCapsule Component1 { get; }
        ICapsuleCollideWithCapsule Component2 { get; }
    }

    public static class CapsuleCapsuleCollisionTupleExtensions
    {
        public static void Deconstruct(this ICapsuleCapsuleCollisionTuple t, out ICapsuleCollideWithCapsule c1, out ICapsuleCollideWithCapsule c2) => _ = (c1 = t.Component1, c2 = t.Component2);
    }
    #endregion

    #region Point-line static collisions
    public interface IPointLineCollisionTuple : IComponent
    {
        public float Friction { get; }
        IPointCollideWithPlane PointComponent { get; }
        ILineCollideWithPoint LineComponent { get; }
    }

    public interface IPointCollideWithPlane : IComponent
    {
        float CollisionRadius { get; }
        Ref<NativeIndexedArray<Id<Point>, float2>> PredictedPositions { get; }
        NativeIndexedArray<Id<Point>, float2>.ReadOnly Positions { get; }
        float Friction { get; }
    }

    public interface ILineCollideWithPoint : IComponent
    {
        Line Line { get; }
        float2 Displacement { get; }
    }

    public interface ITriMeshPointsCollideWithGroundLine : IPointCollideWithPlane { }
    public interface IGroundLineCollideWithTriMeshPoints : ILineCollideWithPoint
    {
        float Friction { get; }
    }

    public static class PointLineCollisionTupleExtensions
    {
        public static void Deconstruct(this IPointLineCollisionTuple tuple, out IPointCollideWithPlane point, out ILineCollideWithPoint line)
            => _ = (point = tuple.PointComponent, line = tuple.LineComponent);
    }
    #endregion

    public interface IFrictionComponent : IComponent
    {
        Ref<NativeIndexedArray<Id<Point>, Friction>> AccumulatedFriction { get; }
        Ref<NativeIndexedArray<Id<Point>, float2>> PredictedPositions { get; }
        NativeIndexedArray<Id<Point>, float2>.ReadOnly Positions { get; }
    }
}