using andywiecko.BurstCollections;

namespace andywiecko.PBD2D.Core
{
    public readonly struct TriangleAreaConstraint : ITriangle
    {
        public readonly Id<Point> IdA { get; }
        public readonly Id<Point> IdB { get; }
        public readonly Id<Point> IdC { get; }
        public readonly float RestArea2 { get; }

        public TriangleAreaConstraint(Id<Point> idA, Id<Point> idB, Id<Point> idC, float restArea2) =>
            (IdA, IdB, IdC, RestArea2) = (idA, idB, idC, restArea2);

        public void Deconstruct(out Id<Point> idA, out Id<Point> idB, out Id<Point> idC, out float restArea2) =>
             (idA, idB, idC, restArea2) = (IdA, IdB, IdC, RestArea2);

        public static implicit operator TriangleAreaConstraint((int idA, int idB, int idC, float restArea2) tuple) =>
             new((Id<Point>)tuple.idA, (Id<Point>)tuple.idB, (Id<Point>)tuple.idC, tuple.restArea2);

        public TriangleAreaConstraint With(float area2) => new(IdA, IdB, IdC, area2);
    }
}
