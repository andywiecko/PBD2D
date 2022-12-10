using andywiecko.BurstCollections;
using andywiecko.BurstMathUtils;
using Unity.Mathematics;

namespace andywiecko.PBD2D.Core
{
    public readonly struct StencilBendingConstraint : ITriangle
    {
        public readonly Id<Point> IdA { get; }
        public readonly Id<Point> IdB { get; }
        public readonly Id<Point> IdC { get; }
        public readonly float RestAngle;
        public StencilBendingConstraint(Id<Point> idA, Id<Point> idB, Id<Point> idC, float restAngle) =>
            (IdA, IdB, IdC, RestAngle) = (idA, idB, idC, restAngle);
        public void Deconstruct(out Id<Point> idA, out Id<Point> idB, out Id<Point> idC, out float restAngle) =>
            (idA, idB, idC, restAngle) = (IdA, IdB, IdC, RestAngle);
        public static StencilBendingConstraint Create<T>(T s, NativeIndexedArray<Id<Point>, float2> positions)
            where T : unmanaged, ITriangle
        {
            var (pA, pB, pC) = positions.At(s);
            var (t1, t2) = (pB - pA, pC - pB);
            var angle = MathUtils.Angle(t1, t2);
            return new(s.IdA, s.IdB, s.IdC, angle);
        }
    }
}