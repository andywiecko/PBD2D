using andywiecko.BurstCollections;
using Unity.Collections;
using Unity.Mathematics;

namespace andywiecko.PBD2D.Core
{
    public interface IBoundingVolumeTreeComponent<T> : IComponent where T : struct, IConvertableToAABB
    {
        float Margin { get; }
        Ref<NativeBoundingVolumeTree<AABB>> Tree { get; }
        Ref<NativeArray<AABB>> Volumes { get; }
        Ref<NativeArray<T>> Objects { get; }
        Ref<NativeIndexedArray<Id<Point>, float2>> Positions { get; }
    }

    public interface IBoundingVolumeTreesIntersectionTuple : IComponent
    {
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
        IPointCollideWithPlane PointComponent { get; }
        ILineCollideWithPoint LineComponent { get; }
    }

    public interface IPointCollideWithPlane : IComponent
    {
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
}