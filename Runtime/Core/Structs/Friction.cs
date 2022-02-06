using Unity.Mathematics;

namespace andywiecko.PBD2D.Core
{
    public readonly struct Friction
    {
        public readonly float2 Value => Normal * Length;
        public readonly float2 Normal => math.normalizesafe(value.xy);
        public readonly float Length => value.z;
        private readonly float4 value; // w is unused, float4 is better for cpu than float3
        public Friction(float2 vector) => value = new(vector, math.length(vector));
        private Friction(float4 value) => this.value = value;
        public static Friction operator +(Friction f0, Friction f1) => new(f0.value + f1.value);
    }
}