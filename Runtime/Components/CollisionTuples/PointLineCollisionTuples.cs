using andywiecko.ECS;
using andywiecko.PBD2D.Core;

namespace andywiecko.PBD2D.Components
{
    public class TriMeshPointsGroundLineCollisionTuple : ComponentsTuple<
        ITriMeshPointsCollideWithGroundLine, IGroundLineCollideWithTriMeshPoints>,
        IPointLineCollisionTuple
    {
        public TriMeshPointsGroundLineCollisionTuple(ITriMeshPointsCollideWithGroundLine item1, IGroundLineCollideWithTriMeshPoints item2, ComponentsRegistry componentsRegistry) : base(item1, item2, componentsRegistry)
        {
        }

        public float Friction => 0.5f * (Item1.Friction + Item2.Friction);
        public IPointCollideWithLine PointComponent => Item1;
        public ILineCollideWithPoint LineComponent => Item2;
    }
}