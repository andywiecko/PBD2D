using andywiecko.BurstCollections;
using Unity.Mathematics;

namespace andywiecko.PBD2D.Core
{
    public readonly struct EdgeEdgeContactInfo
    {
        public readonly float2 BarPointA;
        public readonly float2 BarPointB;
        public readonly Id<Edge> EdgeIdA;
        public readonly Id<Edge> EdgeIdB;

        public EdgeEdgeContactInfo(float2 barPointA, float2 barPointB, Id<Edge> edgeIdA, Id<Edge> edgeIdB)
        {
            BarPointA = barPointA;
            BarPointB = barPointB;
            EdgeIdA = edgeIdA;
            EdgeIdB = edgeIdB;
        }

        public void Deconstruct(out float2 pointA, out float2 pointB, out Id<Edge> edgeIdA, out Id<Edge> edgeIdB)
        {
            pointA = BarPointA;
            pointB = BarPointB;
            edgeIdA = EdgeIdA;
            edgeIdB = EdgeIdB;
        }
    }
}