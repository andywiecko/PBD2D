using andywiecko.BurstCollections;

namespace andywiecko.PBD2D.Core
{
    public readonly struct EdgePair
    {
        public readonly Id<Edge> IdA;
        public readonly Id<Edge> IdB;
        public EdgePair(Id<Edge> idA, Id<Edge> idB) => _ = (IdA = idA, IdB = idB);
        public void Deconstruct(out Id<Edge> idA, out Id<Edge> idB) => _ = (idA = IdA, idB = IdB);
        public static implicit operator EdgePair((Id<Edge> a, Id<Edge> b) tuple) => new(tuple.a, tuple.b);
    }

    public readonly struct ExternalEdgePair
    {
        public readonly Id<ExternalEdge> IdA;
        public readonly Id<ExternalEdge> IdB;

        public ExternalEdgePair(Id<ExternalEdge> idA, Id<ExternalEdge> idB)
        {
            IdA = idA;
            IdB = idB;
        }

        public void Deconstruct(out Id<ExternalEdge> idA, out Id<ExternalEdge> idB)
        {
            idA = IdA;
            idB = IdB;
        }

        public static implicit operator ExternalEdgePair((Id<ExternalEdge> a, Id<ExternalEdge> b) tuple) => new ExternalEdgePair(tuple.a, tuple.b);
    }
}
