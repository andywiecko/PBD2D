using andywiecko.BurstCollections;
using Unity.Mathematics;

namespace andywiecko.PBD2D.Core
{
    public interface IConvertableToAABB
    {
        AABB ToAABB(NativeIndexedArray<Id<Point>, float2>.ReadOnly positions, float margin = 0);
    }
}
