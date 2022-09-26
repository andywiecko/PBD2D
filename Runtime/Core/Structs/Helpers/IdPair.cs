using andywiecko.BurstCollections;
using System;

namespace andywiecko.PBD2D.Core
{
    public readonly struct IdPair<T> : IEquatable<IdPair<T>>
    {
        public readonly Id<T> Id1, Id2;
        public IdPair(Id<T> id1, Id<T> id2) => (Id1, Id2) = (id1, id2);
        public readonly void Deconstruct(out Id<T> id1, out Id<T> id2) => (id1, id2) = (Id1, Id2);
        public static implicit operator IdPair<T>((Id<T> id1, Id<T> id2) tuple) => new(tuple.id1, tuple.id2);
        public override string ToString() => $"({Id1}, {Id2})";
        public bool Equals(IdPair<T> other) => Id1 == other.Id1 && Id2 == other.Id2;
    }

    public readonly struct IdPair<T1, T2> : IEquatable<IdPair<T1, T2>>
    {
        public readonly Id<T1> Id1;
        public readonly Id<T2> Id2;
        public IdPair(Id<T1> id1, Id<T2> id2) => (Id1, Id2) = (id1, id2);
        public readonly void Deconstruct(out Id<T1> id1, out Id<T2> id2) => (id1, id2) = (Id1, Id2);
        public static implicit operator IdPair<T1, T2>((Id<T1> id1, Id<T2> id2) tuple) => new(tuple.id1, tuple.id2);
        public override string ToString() => $"({Id1}, {Id2})";
        public bool Equals(IdPair<T1, T2> other) => Id1 == other.Id1 && Id2 == other.Id2;
    }
}