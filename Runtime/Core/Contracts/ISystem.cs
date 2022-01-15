using Unity.Jobs;

namespace andywiecko.PBD2D.Core
{
    public interface ISystem
    {
        JobHandle Schedule(JobHandle dependencies);
    }
}