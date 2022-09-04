using andywiecko.BurstCollections;
using System;
using Unity.Mathematics;

namespace andywiecko.PBD2D.Core
{
    public interface IEdge
    {
        Id<Point> IdA { get; }
        Id<Point> IdB { get; }
    }

    public static class IEdgeExtensions
    {
        public static Edge ToEdge<T>(this T edge) where T : unmanaged, IEdge => (edge.IdA, edge.IdB);
        public static void Deconstruct<T>(this T edge, out Id<Point> idA, out Id<Point> idB) where T : unmanaged, IEdge
            => (idA, idB) = (edge.IdA, edge.IdB);
        public static float2 GetCenter<T>(this T edge, NativeIndexedArray<Id<Point>, float2>.ReadOnly positions) where T : unmanaged, IEdge =>
            0.5f * (positions[edge.IdA] + positions[edge.IdB]);
        public static float2 GetCenter<T>(this T edge, NativeIndexedArray<Id<Point>, float2> positions) where T : unmanaged, IEdge =>
            GetCenter(edge, positions.AsReadOnly());
        public static float GetLength<T>(this T edge, NativeIndexedArray<Id<Point>, float2>.ReadOnly positions) where T : unmanaged, IEdge =>
            math.distance(positions[edge.IdA], positions[edge.IdB]);
        public static float GetLength<T>(this T edge, NativeIndexedArray<Id<Point>, float2> positions) where T : unmanaged, IEdge =>
            GetLength(edge, positions.AsReadOnly());
        public static AABB ToAABB<T>(this T edge, NativeIndexedArray<Id<Point>, float2>.ReadOnly positions, float margin = 0)
            where T : unmanaged, IEdge
        {
            var (pA, pB) = positions.At(edge);
            return new
            (
                min: math.min(pA, pB) - margin,
                max: math.max(pA, pB) + margin
            );
        }
    }

    public readonly struct Edge : IEquatable<Edge>, IEdge, IConvertableToAABB
    {
        public readonly Id<Point> IdA { get; }
        public readonly Id<Point> IdB { get; }
        private Edge(int idA, int idB) => (IdA, IdB) = ((Id<Point>)idA, (Id<Point>)idB);
        public Edge(Id<Point> idA, Id<Point> idB) : this((int)idA, (int)idB) { }
        public static implicit operator Edge((int idA, int idB) ids) => new Edge(ids);
        public static implicit operator Edge((Id<Point> idA, Id<Point> idB) ids) => new Edge(ids);
        private Edge((int idA, int idB) ids) : this((Id<Point>)ids.idA, (Id<Point>)ids.idB) { }
        private Edge((Id<Point> idA, Id<Point> idB) ids) : this(ids.idA, ids.idB) { }
        public bool Equals(Edge other) => IdA == other.IdA && IdB == other.IdB;
        public bool Contains(Id<Point> id) => IdA == id || IdB == id;
        public override string ToString() => $"({nameof(Edge)})({IdA}, {IdB})";
        AABB IConvertableToAABB.ToAABB(NativeIndexedArray<Id<Point>, float2>.ReadOnly positions, float margin) => this.ToAABB(positions, margin);
    }
}
