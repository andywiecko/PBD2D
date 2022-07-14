using Unity.Mathematics;

namespace andywiecko.PBD2D.Systems
{
    public static class FrictionUtils
    {
        public static (float2 dpA, float2 dpB) GetFrictionCorrections(
            float2 pA, float2 qA, float wA,
            float2 pB, float2 qB, float wB, float2 n,
            float mu, float2 fn)
        {
            var wAB = wA + wB;
            if (wAB == 0)
            {
                return (0, 0);
            }

            var dx = (pA - qA) - (pB - qB);
            var dxn = n * math.dot(n, dx);
            var dxt = dx - dxn;
            dx = math.min(math.length(dxt), mu * math.length(fn)) * math.normalizesafe(dxt);
            return (-wA / wAB * dx, +wB / wAB * dx);
        }
    }
}