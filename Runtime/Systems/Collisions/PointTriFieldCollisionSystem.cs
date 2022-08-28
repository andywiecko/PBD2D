using andywiecko.ECS;
using andywiecko.PBD2D.Core;

namespace andywiecko.PBD2D.Systems
{
    [Category(PBDCategory.Collisions)]
    public class PointTriFieldCollisionSystem : PointTriFieldCollisionSystem<TriFieldLookup.ReadOnly> { }
}