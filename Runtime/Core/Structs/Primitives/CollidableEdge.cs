using andywiecko.BurstCollections;

namespace andywiecko.PBD2D.Core
{
    public readonly struct CollidableEdge : IEdge
    {
        public readonly Id<Point> IdA { get; }
        public readonly Id<Point> IdB { get; }
        public CollidableEdge(Id<Point> idA, Id<Point> idB) => (IdA, IdB) = (idA, idB);
        public static implicit operator CollidableEdge((int i, int j) tuple) => new((Id<Point>)tuple.i, (Id<Point>)tuple.j);
        public static implicit operator Edge(CollidableEdge e) => e.ToEdge();
    }
}