using andywiecko.BurstCollections;
using Unity.Mathematics;

namespace andywiecko.PBD2D.Core
{
    public readonly struct EdgeEdgeContactInfo
    {
        public readonly float2 BarPointA, BarPointB;
        public readonly Id<CollidableEdge> EdgeIdA, EdgeIdB;

        public EdgeEdgeContactInfo(float2 barPointA, float2 barPointB, Id<CollidableEdge> edgeIdA, Id<CollidableEdge> edgeIdB) =>
            (BarPointA, BarPointB, EdgeIdA, EdgeIdB) = (barPointA, barPointB, edgeIdA, edgeIdB);

        public void Deconstruct(out float2 barPointA, out float2 barPointB, out Id<CollidableEdge> edgeIdA, out Id<CollidableEdge> edgeIdB) =>
            (barPointA, barPointB, edgeIdA, edgeIdB) = (BarPointA, BarPointB, EdgeIdA, EdgeIdB);
    }

    public readonly struct EdgeEdgeContactInfo2
    {
        public readonly float2 BarPointA, BarPointB;
        public readonly Id<ExternalEdge> EdgeIdA, EdgeIdB;

        public EdgeEdgeContactInfo2(float2 barPointA, float2 barPointB, Id<ExternalEdge> edgeIdA, Id<ExternalEdge> edgeIdB) =>
            (BarPointA, BarPointB, EdgeIdA, EdgeIdB) = (barPointA, barPointB, edgeIdA, edgeIdB);

        public void Deconstruct(out float2 barPointA, out float2 barPointB, out Id<ExternalEdge> edgeIdA, out Id<ExternalEdge> edgeIdB) =>
            (barPointA, barPointB, edgeIdA, edgeIdB) = (BarPointA, BarPointB, EdgeIdA, EdgeIdB);
    }
}