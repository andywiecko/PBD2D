using Unity.Mathematics;
using UnityEngine;

namespace andywiecko.PBD2D.Core
{
    public static class MathematicsExtensions
    {
        public static float3 ToFloat3(this float2 value) => math.float3(value, 0f);
        public static float2 ToFloat2(this Vector3 value) => ((float3)(value)).xy;
        public static void Deconstruct(this float2 value, out float x, out float y) => _ = (x = value.x, y = value.y);
    }
}