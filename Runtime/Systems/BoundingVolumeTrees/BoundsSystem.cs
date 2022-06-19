using andywiecko.ECS;
using andywiecko.PBD2D.Core;
using Unity.Jobs;

namespace andywiecko.PBD2D.Systems
{
    [Category(PBDCategory.ExtendedData)]
    public class BoundsSystem : BaseSystem<IBoundsComponent>
    {
        // TODO: introduce ActionSystem without schedule
        public override JobHandle Schedule(JobHandle dependencies)
        {
            throw new System.NotImplementedException();
        }

        [SolverAction]
        public void UpdateBounds()
        {
            for (int i = 0; i < References.Count; i++)
            {
                References[i].UpdateBounds();
            }
        }
    }
}