using Unity.Mathematics;

namespace andywiecko.PBD2D.Core
{
    public readonly struct Poly6Kernel
    {
        private readonly float R2;
        private readonly float PolyTerm;

        public Poly6Kernel(float interactionRadius)
        {
            var R = interactionRadius;
            R2 = R * R;
            var R3 = R2 * R;
            var R9 = R3 * R3 * R3;
            PolyTerm = 315f / (64f * math.PI * R9);
        }

        public float Value(float r)
        {
            var r2 = r * r;
            var R2r2 = R2 - r2;
            return R2r2 < 0 || r == 0 ? 0 : PolyTerm * R2r2 * R2r2 * R2r2;
        }

        public float Value(float2 pi, float2 pj)
        {
            var pij2 = math.distancesq(pi, pj);
            var R2pij2 = R2 - pij2;
            return R2pij2 < 0 || pij2 == 0 ? 0 : PolyTerm * R2pij2 * R2pij2 * R2pij2;
        }
    }

    public readonly struct SpikyKernel
    {
        private readonly float R;
        private readonly float SpikyTerm;

        public SpikyKernel(float interactionRadius)
        {
            R = interactionRadius;
            var R2 = R * R;
            var R3 = R2 * R;
            var R6 = R3 * R3;
            SpikyTerm = 45f / (math.PI * R6);
        }

        public float2 Value(float2 pi, float2 pj)
        {
            var r = pi - pj;
            var rLen = math.length(r);
            var Rr = R - rLen;

            return Rr < 0 || rLen == 0 ? 0 : -SpikyTerm * Rr * Rr * math.normalize(r);
        }
    }
}
