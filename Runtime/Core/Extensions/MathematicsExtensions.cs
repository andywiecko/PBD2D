using Unity.Mathematics;
using UnityEngine;

namespace andywiecko.PBD2D.Core
{
    public static class MathematicsExtensions
    {
        public static float3 ToFloat3(this float2 value, float z = default) => math.float3(value, z);
        public static float2 ToFloat2(this Vector3 value) => ((float3)(value)).xy;
        public static void Deconstruct(this float2 value, out float x, out float y) => (x, y) = (value.x, value.y);
        public static void Deconstruct(this int2 value, out int x, out int y) => (x, y) = (value.x, value.y);
    }
}