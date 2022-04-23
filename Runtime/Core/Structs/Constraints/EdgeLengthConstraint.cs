using andywiecko.BurstCollections;

namespace andywiecko.PBD2D.Core
{
    public readonly struct EdgeLengthConstraint : IEdge
    {
        public readonly Id<Point> IdA { get; }
        public readonly Id<Point> IdB { get; }
        public readonly float RestLength { get; }
        private readonly int layout { get; }

        public EdgeLengthConstraint(Id<Point> idA, Id<Point> idB, float restLength) =>
            (IdA, IdB, RestLength, layout) = (idA, idB, restLength, 0);

        public void Deconstruct(out Id<Point> idA, out Id<Point> idB, out float restLength) =>
             (idA, idB, restLength) = (IdA, IdB, RestLength);

        public static implicit operator EdgeLengthConstraint((int idA, int idB, float restLength) tuple) =>
            new((Id<Point>)tuple.idA, (Id<Point>)tuple.idB, tuple.restLength);
    }
}