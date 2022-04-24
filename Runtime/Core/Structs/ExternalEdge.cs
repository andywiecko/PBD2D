using andywiecko.BurstCollections;
using andywiecko.BurstMathUtils;
using System;
using Unity.Mathematics;

namespace andywiecko.PBD2D.Core
{
    public readonly struct ExternalEdge : IEquatable<ExternalEdge>, IEdge
    {
        public readonly Id<Point> IdA { get; }
        public readonly Id<Point> IdB { get; }
        public ExternalEdge(Id<Point> idA, Id<Point> idB) => (IdA, IdB) = (idA, idB);

        public float2 GetNormal(NativeIndexedArray<Id<Point>, float2>.ReadOnly positions)
        {
            var (pA, pB) = (positions[IdA], positions[IdB]);
            return math.normalizesafe(MathUtils.Rotate90CW(pB - pA));
        }

        public bool Equals(ExternalEdge other) => IdA == other.IdA && IdB == other.IdB;
    }
}
