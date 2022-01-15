using andywiecko.BurstCollections;

namespace andywiecko.PBD2D.Core
{
    public readonly struct PointTrianglePair
    {
        public readonly Id<Point> PointId;
        public readonly Id<Triangle> TriangleId;

        public PointTrianglePair(Id<Point> pointId, Id<Triangle> triangleId)
        {
            PointId = pointId;
            TriangleId = triangleId;
        }

        public void Deconstruct(out Id<Point> pointId, out Id<Triangle> triangleId)
        {
            pointId = PointId;
            triangleId = TriangleId;
        }

        public static implicit operator PointTrianglePair((Id<Point> pId, Id<Triangle> tId) pair) => new PointTrianglePair(pair.pId, pair.tId);
    }
}