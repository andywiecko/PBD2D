using Unity.Jobs;

namespace andywiecko.PBD2D.Core
{
    public interface ISystem
    {
        IWorld World { get; set; }
        JobHandle Schedule(JobHandle dependencies);
    }
}