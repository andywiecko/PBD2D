using andywiecko.BurstCollections;

namespace andywiecko.PBD2D.Core
{
    public readonly struct Stencil : ITriangle
    {
        public readonly Id<Point> IdA { get; }
        public readonly Id<Point> IdB { get; }
        public readonly Id<Point> IdC { get; }
        public Stencil(Id<Point> idA, Id<Point> idB, Id<Point> idC) => (IdA, IdB, IdC) = (idA, idB, idC);
        public override string ToString() => $"({IdA}, {IdB}, {IdC})";
    }
}
