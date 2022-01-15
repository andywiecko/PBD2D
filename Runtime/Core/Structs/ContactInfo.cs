using andywiecko.BurstCollections;
using Unity.Mathematics;

namespace andywiecko.PBD2D.Core
{
    public readonly struct ContactInfo
    {
        public readonly float2 PointA;
        public readonly float2 PointB;
        public readonly Id<Edge> EdgeIdA;
        public readonly Id<Edge> EdgeIdB;

        public ContactInfo(float2 pointA, float2 pointB, Id<Edge> edgeIdA, Id<Edge> edgeIdB)
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

    public readonly struct ExternalEdgesContactInfo
    {
        public readonly float2 PointA;
        public readonly float2 PointB;
        public readonly Id<ExternalEdge> EdgeIdA;
        public readonly Id<ExternalEdge> EdgeIdB;

        public ExternalEdgesContactInfo(float2 pointA, float2 pointB, Id<ExternalEdge> edgeIdA, Id<ExternalEdge> edgeIdB)
        {
            PointA = pointA;
            PointB = pointB;
            EdgeIdA = edgeIdA;
            EdgeIdB = edgeIdB;
        }

        public void Deconstruct(out float2 pointA, out float2 pointB, out Id<ExternalEdge> edgeIdA, out Id<ExternalEdge> edgeIdB)
        {
            pointA = PointA;
            pointB = PointB;
            edgeIdA = EdgeIdA;
            edgeIdB = EdgeIdB;
        }
    }
}