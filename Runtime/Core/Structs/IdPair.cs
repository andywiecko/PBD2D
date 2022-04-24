using andywiecko.BurstCollections;

namespace andywiecko.PBD2D.Core
{
    public readonly struct IdPair<T>
    {
        public readonly Id<T> Id1;
        public readonly Id<T> Id2;
        public IdPair(Id<T> id1, Id<T> id2) => _ = (Id1 = id1, Id2 = id2);
        public readonly void Deconstruct(out Id<T> id1, out Id<T> id2) => _ = (id1 = Id1, id2 = Id2);
        public static implicit operator IdPair<T>((Id<T> id1, Id<T> id2) tuple) => new(tuple.id1, tuple.id2);
    }

    public readonly struct IdPair<T1, T2>
    {
        public readonly Id<T1> Id1;
        public readonly Id<T2> Id2;
        public IdPair(Id<T1> id1, Id<T2> id2) => _ = (Id1 = id1, Id2 = id2);
        public readonly void Deconstruct(out Id<T1> id1, out Id<T2> id2) => _ = (id1 = Id1, id2 = Id2);
        public static implicit operator IdPair<T1, T2>((Id<T1> id1, Id<T2> id2) tuple) => new(tuple.id1, tuple.id2);
    }
}