using andywiecko.BurstCollections;
using System;

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
        public static void Deconstruct<T>(this T triangle, out Id<Point> idA, out Id<Point> idB, out Id<Point> idC) where T : struct, ITriangle
            => (idA, idB, idC) = (triangle.IdA, triangle.IdB, triangle.IdC);
    }

    [Serializable]
    public readonly struct Triangle : IEquatable<Triangle>, ITriangle
    {
        public static Triangle Disabled => new Triangle(Id<Point>.Invalid, Id<Point>.Invalid, Id<Point>.Invalid);

        public bool IsEnabled => !Equals(Disabled);

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

        public Triangle(Id<Point> idA, Id<Point> idB, Id<Point> idC)
        {
            IdA = idA;
            IdB = idB;
            IdC = idC;
        }

        public static implicit operator Triangle((int idA, int idB, int idC) ids) => new Triangle(ids);
        public static implicit operator Triangle((Id<Point> idA, Id<Point> idB, Id<Point> idC) ids) => new Triangle(ids.idA, ids.idB, ids.idC);

        private Triangle((int idA, int idB, int idC) ids) : this((Id<Point>)ids.idA, (Id<Point>)ids.idB, (Id<Point>)ids.idC) { }

        public void Deconstruct(out Id<Point> idA, out Id<Point> idB, out Id<Point> idC)
        {
            idA = IdA;
            idB = IdB;
            idC = IdC;
        }

        public bool Equals(Triangle other) => IdA == other.IdA && IdB == other.IdB && IdC == other.IdC;

        public Id<Point> Other(Edge edge)
        {
            if (!Contains(edge))
            {
                return Id<Point>.Invalid;
            }

            if (!edge.Contains(IdA))
            {
                return IdA;
            }
            else if (!edge.Contains(IdB))
            {
                return IdB;
            }
            else
            {
                return IdC;
            }
        }

        public Id<Point> Other(Id<Point> idA, Id<Point> idB) => Other((idA, idB));

        public bool Contains(Id<Point> pointId) => IdA == pointId || IdB == pointId || IdC == pointId;
        public bool Contains(Edge edge)
        {
            var (idA, idB) = edge;
            return Contains(idA, idB);
        }

        public bool Contains(Id<Point> idA, Id<Point> idB) => Contains(idA) && Contains(idB);

        public bool ContainsCommonPointWith(Edge edge)
        {
            var (idA, idB) = edge;
            return Contains(idA) || Contains(idB);
        }

        public bool ContainsCommonPointWith(Triangle triangle)
        {
            var (idA, idB, idC) = triangle;
            return Contains(idA) || Contains(idB) || Contains(idC);
        }

        public override string ToString() => $"{nameof(Triangle)}({IdA}, {IdB}, {IdC})";
    }
}
