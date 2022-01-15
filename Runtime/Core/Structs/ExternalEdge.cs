using andywiecko.BurstCollections;
using andywiecko.BurstMathUtils;
using System;
using Unity.Mathematics;

namespace andywiecko.PBD2D.Core
{
    public readonly struct ExternalEdge : IEquatable<ExternalEdge>
    {
        public static ExternalEdge Disabled => new ExternalEdge(Id<Point>.Invalid, Id<Point>.Invalid);
        public bool IsEnabled => !Equals(Disabled);

        public readonly Id<Point> IdA;
        public readonly Id<Point> IdB;

        public ExternalEdge(Id<Point> idA, Id<Point> idB)
        {
            IdA = idA;
            IdB = idB;
        }

        public void Deconstruct(out Id<Point> idA, out Id<Point> idB)
        {
            idA = IdA;
            idB = IdB;
        }

        public float2 GetNormal(NativeIndexedArray<Id<Point>, float2>.ReadOnly positions)
        {
            var (pA, pB) = (positions[IdA], positions[IdB]);
            return math.normalizesafe(MathUtils.Rotate90CW(pB - pA));
        }

        public bool Equals(ExternalEdge other) => IdA == other.IdA && IdB == other.IdB;
    }
}
