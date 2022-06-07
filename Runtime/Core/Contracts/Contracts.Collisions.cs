using andywiecko.BurstCollections;
using andywiecko.ECS;
using Unity.Collections;
using Unity.Mathematics;

namespace andywiecko.PBD2D.Core
{
    public interface IBoundsComponent : IComponent
    {
        AABB Bounds { get; }
        void UpdateBounds();
    }

    public interface IBoundingVolumeTreeComponent<T> : IComponent where T : struct, IConvertableToAABB
    {
        float Margin { get; }
        AABB Bounds { get; }
        Ref<NativeBoundingVolumeTree<AABB>> Tree { get; }
        Ref<NativeArray<AABB>> Volumes { get; }
        Ref<NativeArray<T>> Objects { get; }
        Ref<NativeIndexedArray<Id<Point>, float2>> Positions { get; }
    }

    public interface IBoundingVolumeTreesIntersectionTuple : IComponent
    {
        AABB Bounds1 { get; }
        AABB Bounds2 { get; }
        Ref<NativeBoundingVolumeTree<AABB>> Tree1 { get; }
        Ref<NativeBoundingVolumeTree<AABB>> Tree2 { get; }
        Ref<NativeList<int2>> Result { get; }
    }

    #region Capsule-capsule collisions
    public interface ICapsuleCollideWithCapsuleBroadphase : IComponent
    {
        Ref<NativeBoundingVolumeTree<AABB>> Tree { get; }
    }

    public interface ICapsuleCollideWithCapsule : IComponent
    {
        float Friction { get; }
        float CollisionRadius { get; }
        AABB Bounds { get; }
        Ref<NativeIndexedArray<Id<Point>, float2>> PredictedPositions { get; }
        Ref<NativeIndexedArray<Id<Point>, float2>> Positions { get; }
        Ref<NativeIndexedArray<Id<Point>, float>> MassesInv { get; }
        Ref<NativeIndexedArray<Id<CollidableEdge>, CollidableEdge>> CollidableEdges { get; }
    }

    public interface ITriMeshCapsulesCollideWithTriMeshCapsules : ICapsuleCollideWithCapsule, ICapsuleCollideWithCapsuleBroadphase { }
    public interface IRodCapsulesCollideWithTriMeshCapsules : ICapsuleCollideWithCapsule { }
    public interface ITriMeshCapsulesCollideWithRodCapsules : ICapsuleCollideWithCapsule { }

    public interface ICapsuleCapsuleCollisionTuple : IComponent
    {
        float Friction { get; }
        Ref<NativeList<IdPair<CollidableEdge>>> PotentialCollisions { get; }
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
        IPointCollideWithLine PointComponent { get; }
        ILineCollideWithPoint LineComponent { get; }
    }

    public interface IPointCollideWithLine : IComponent
    {
        AABB Bounds { get; }
        float CollisionRadius { get; }
        Ref<NativeIndexedArray<Id<Point>, float2>> PredictedPositions { get; }
        Ref<NativeIndexedArray<Id<Point>, float2>> Positions { get; }
        float Friction { get; }
    }

    public interface ILineCollideWithPoint : IComponent
    {
        Line Line { get; }
        float2 Displacement { get; }
    }

    public interface ITriMeshPointsCollideWithGroundLine : IPointCollideWithLine { }
    public interface IGroundLineCollideWithTriMeshPoints : ILineCollideWithPoint
    {
        float Friction { get; }
    }

    public static class PointLineCollisionTupleExtensions
    {
        public static void Deconstruct(this IPointLineCollisionTuple tuple, out IPointCollideWithLine point, out ILineCollideWithPoint line)
            => _ = (point = tuple.PointComponent, line = tuple.LineComponent);
    }
    #endregion

    #region Point-TriField collisions
    public interface IPointCollideWithTriFieldBroadphase : IEntityComponent
    {
        Ref<NativeBoundingVolumeTree<AABB>> Tree { get; }
    }

    public interface IPointCollideWithTriField : IEntityComponent
    {
        AABB Bounds { get; }
        Ref<NativeIndexedArray<Id<Point>, float2>> PredictedPositions { get; }
        Ref<NativeIndexedArray<Id<Point>, float>> MassesInv { get; }
    }

    public interface ITriFieldCollideWithPointBroadphase : IEntityComponent
    {
        Ref<NativeBoundingVolumeTree<AABB>> Tree { get; }
    }

    public interface ITriFieldCollideWithPoint : IEntityComponent
    {
        AABB Bounds { get; }
        Ref<NativeIndexedArray<Id<Point>, float2>> PredictedPositions { get; }
        Ref<NativeIndexedArray<Id<Point>, float>> MassesInv { get; }
        Ref<NativeIndexedArray<Id<Triangle>, Triangle>> Triangles { get; }
        Ref<NativeIndexedArray<Id<ExternalEdge>, ExternalEdge>> ExternalEdges { get; }
        Ref<TriFieldLookup> TriFieldLookup { get; }
    }

    public interface IPointTriFieldCollisionTuple : IComponent
    {
        AABB Bounds1 { get; }
        AABB Bounds2 { get; }
        Ref<NativeList<IdPair<Point, Triangle>>> PotentialCollisions { get; }
        Ref<NativeList<IdPair<Point, ExternalEdge>>> Collisions { get; }
        IPointCollideWithTriField PointsComponent { get; }
        ITriFieldCollideWithPoint TriFieldComponent { get; }
    }

    public interface ITriMeshPointsCollideWithTriMeshTriField : IPointCollideWithTriField, IPointCollideWithTriFieldBroadphase { }
    public interface ITriMeshTriFieldCollideWithTriMeshPoints : ITriFieldCollideWithPoint, ITriFieldCollideWithPointBroadphase { }
    #endregion
}