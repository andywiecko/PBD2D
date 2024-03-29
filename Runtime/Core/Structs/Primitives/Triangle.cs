using andywiecko.BurstCollections;
using andywiecko.BurstMathUtils;
using System;
using Unity.Mathematics;

namespace andywiecko.PBD2D.Core
{
    public interface ITriangle
    {
        Id<Point> IdA { get; }
        Id<Point> IdB { get; }
        Id<Point> IdC { get; }
    }

    public static class ITriangleExtensions
    {
        public static Triangle ToTriangle<T>(this T triangle) where T : unmanaged, ITriangle => (triangle.IdA, triangle.IdB, triangle.IdC);
        public static void Deconstruct<T>(this T triangle, out Id<Point> idA, out Id<Point> idB, out Id<Point> idC)
            where T : unmanaged, ITriangle => (idA, idB, idC) = (triangle.IdA, triangle.IdB, triangle.IdC);
        public static AABB ToAABB<T>(this T triangle, NativeIndexedArray<Id<Point>, float2>.ReadOnly positions, float margin = 0)
            where T : unmanaged, ITriangle
        {
            var (pA, pB, pC) = positions.At(triangle);
            return new
            (
                min: MathUtils.Min(pA, pB, pC) - margin,
                max: MathUtils.Max(pA, pB, pC) + margin
            );
        }
        public static float GetSignedArea2<T>(this T triangle, ReadOnlySpan<float2> positions) where T : unmanaged, ITriangle
        {
            var (pIdA, pIdB, pIdC) = triangle;
            var (pA, pB, pC) = (positions[(int)pIdA], positions[(int)pIdB], positions[(int)pIdC]);
            return MathUtils.TriangleSignedArea2(pA, pB, pC);
        }
        public static float GetSignedArea2<T>(this T triangle, NativeIndexedArray<Id<Point>, float2>.ReadOnly positions)
            where T : unmanaged, ITriangle => GetSignedArea2<T>(triangle, positions.AsReadOnlySpan());
    }

    public readonly struct Triangle : IEquatable<Triangle>, ITriangle, IConvertableToAABB
    {
        public readonly Id<Point> IdA { get; }
        public readonly Id<Point> IdB { get; }
        public readonly Id<Point> IdC { get; }

        public Id<Point> this[int pId] => pId switch
        {
            0 => IdA,
            1 => IdB,
            2 => IdC,
            _ => throw new IndexOutOfRangeException()
        };

        public Triangle(Id<Point> idA, Id<Point> idB, Id<Point> idC) => (IdA, IdB, IdC) = (idA, idB, idC);
        public static implicit operator Triangle((int idA, int idB, int idC) ids) => new((Id<Point>)ids.idA, (Id<Point>)ids.idB, (Id<Point>)ids.idC);
        public static implicit operator Triangle((Id<Point> idA, Id<Point> idB, Id<Point> idC) ids) => new(ids.idA, ids.idB, ids.idC);
        public bool Equals(Triangle other) => IdA == other.IdA && IdB == other.IdB && IdC == other.IdC;
        public bool Contains(Id<Point> pointId) => IdA == pointId || IdB == pointId || IdC == pointId;
        public bool Contains(Edge edge) => Contains(edge.IdA, edge.IdB);
        public bool Contains(Id<Point> idA, Id<Point> idB) => Contains(idA) && Contains(idB);
        public override string ToString() => $"{nameof(Triangle)}({IdA}, {IdB}, {IdC})";
        AABB IConvertableToAABB.ToAABB(NativeIndexedArray<Id<Point>, float2>.ReadOnly positions, float margin) => this.ToAABB(positions, margin);
    }
}
