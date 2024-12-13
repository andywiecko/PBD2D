using Unity.Entities;

namespace andywiecko.PBD2D
{
    public class PBDRateManager : IRateManager
    {
        public float Timestep { get; set; }

        const int max = 16;
        int count = 0;

        public bool ShouldGroupUpdate(ComponentSystemGroup group)
        {
            if (count++ < max)
            {
                return true;
            }
            else
            {
                count = 0;
                return false;
            }
        }
    }

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial class PBDSystemGroup : ComponentSystemGroup { }

    [UpdateInGroup(typeof(PBDSystemGroup))]
    [UpdateBefore(typeof(PBDItersSystemGroup))]
    public partial class PBDStartSystemGroup : ComponentSystemGroup { }

    [UpdateInGroup(typeof(PBDSystemGroup))]
    public partial class PBDItersSystemGroup : ComponentSystemGroup
    {
        public PBDItersSystemGroup() => RateManager = new PBDRateManager();
    }

    [UpdateInGroup(typeof(PBDSystemGroup))]
    [UpdateAfter(typeof(PBDItersSystemGroup))]
    public partial class PBDEndSystemGroup : ComponentSystemGroup { }
}
