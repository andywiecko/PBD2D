using andywiecko.BurstCollections;
using Unity.Mathematics;

namespace andywiecko.PBD2D.Core
{
    public readonly struct EdgeEdgeContactInfo
    {
        public readonly float2 BarPointA;
        public readonly float2 BarPointB;
        public readonly Id<CollidableEdge> EdgeIdA;
        public readonly Id<CollidableEdge> EdgeIdB;

        public EdgeEdgeContactInfo(float2 barPointA, float2 barPointB, Id<CollidableEdge> edgeIdA, Id<CollidableEdge> edgeIdB)
        {
            BarPointA = barPointA;
            BarPointB = barPointB;
            EdgeIdA = edgeIdA;
            EdgeIdB = edgeIdB;
        }

        public void Deconstruct(out float2 pointA, out float2 pointB, out Id<CollidableEdge> edgeIdA, out Id<CollidableEdge> edgeIdB)
        {
            pointA = BarPointA;
            pointB = BarPointB;
            edgeIdA = EdgeIdA;
            edgeIdB = EdgeIdB;
        }
    }
}