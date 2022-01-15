using andywiecko.PBD2D.Core;

namespace andywiecko.PBD2D.Components
{
    public class TriMeshPointsGroundLineCollisionTuple : ComponentsTuple<
        ITriMeshPointsCollideWithGroundLine, IGroundLineCollideWithTriMeshPoints>,
        IPointLineCollisionTuple
    {
        public IPointCollideWithPlane PointComponent => Item1;
        public ILineCollideWithPoint LineComponent => Item2;
    }
}