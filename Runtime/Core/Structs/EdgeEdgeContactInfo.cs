using andywiecko.BurstCollections;
using Unity.Mathematics;

namespace andywiecko.PBD2D.Core
{
    public readonly struct EdgeEdgeContactInfo
    {
        public readonly float2 PointA;
        public readonly float2 PointB;
        public readonly Id<Edge> EdgeIdA;
        public readonly Id<Edge> EdgeIdB;

        public EdgeEdgeContactInfo(float2 pointA, float2 pointB, Id<Edge> edgeIdA, Id<Edge> edgeIdB)
        {
            PointA = pointA;
            PointB = pointB;
            EdgeIdA = edgeIdA;
            EdgeIdB = edgeIdB;
        }

        public void Deconstruct(out float2 pointA, out float2 pointB, out Id<Edge> edgeIdA, out Id<Edge> edgeIdB)
        {
            pointA = PointA;
            pointB = PointB;
            edgeIdA = EdgeIdA;
            edgeIdB = EdgeIdB;
        }
    }
}