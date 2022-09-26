using andywiecko.BurstCollections;

namespace andywiecko.PBD2D.Core
{
    public readonly struct BendingAngleConstraint : ITriangle
    {
        public readonly Id<Point> IdA { get; }
        public readonly Id<Point> IdB { get; }
        public readonly Id<Point> IdC { get; }
        public readonly float RestAngle;
        public BendingAngleConstraint(Id<Point> idA, Id<Point> idB, Id<Point> idC, float restAngle) =>
            (IdA, IdB, IdC, RestAngle) = (idA, idB, idC, restAngle);
        public void Deconstruct(out Id<Point> idA, out Id<Point> idB, out Id<Point> idC, out float restAngle) =>
            (idA, idB, idC, restAngle) = (IdA, IdB, IdC, RestAngle);
    }
}