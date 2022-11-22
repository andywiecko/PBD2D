using andywiecko.BurstCollections;

namespace andywiecko.PBD2D.Core
{
    public readonly struct PointPointConnectorConstraint
    {
        public Id<Point> IdA { get; }
        public Id<Point> IdB { get; }
        public PointPointConnectorConstraint(Id<Point> idA, Id<Point> idB) => (IdA, IdB) = (idA, idB);
        public void Deconstruct(out Id<Point> idA, out Id<Point> idB) => (idA, idB) = (IdA, IdB);
    }
}