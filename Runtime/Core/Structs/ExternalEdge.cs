using andywiecko.BurstCollections;
using andywiecko.BurstMathUtils;
using System;
using Unity.Mathematics;

namespace andywiecko.PBD2D.Core
{
    public readonly struct ExternalEdge : IEquatable<ExternalEdge>, IEdge, IConvertableToAABB
    {
        public readonly Id<Point> IdA { get; }
        public readonly Id<Point> IdB { get; }
        public ExternalEdge(Id<Point> idA, Id<Point> idB) => (IdA, IdB) = (idA, idB);

        public float2 GetNormal(NativeIndexedArray<Id<Point>, float2> positions) => GetNormal(positions.AsReadOnly());

        public float2 GetNormal(NativeIndexedArray<Id<Point>, float2>.ReadOnly positions)
        {
            var (pA, pB) = (positions[IdA], positions[IdB]);
            return -math.normalizesafe(MathUtils.Rotate90CW(pB - pA));
        }

        public static implicit operator Edge(ExternalEdge edge) => edge.ToEdge();
        public static implicit operator ExternalEdge((int i, int j) tuple) => new((Id<Point>)tuple.i, (Id<Point>)tuple.j);
        public bool Equals(ExternalEdge other) => IdA == other.IdA && IdB == other.IdB;
        public AABB ToAABB(NativeIndexedArray<Id<Point>, float2>.ReadOnly positions, float margin = 0) => this.ToEdge().ToAABB(positions, margin);
    }
}
