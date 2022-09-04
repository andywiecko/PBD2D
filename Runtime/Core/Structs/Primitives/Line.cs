using Unity.Mathematics;

namespace andywiecko.PBD2D.Core
{
    public readonly struct Line
    {
        public readonly float2 Point, Normal;
        public Line(float2 point, float2 normal) => (Point, Normal) = (point, normal);
        public void Deconstruct(out float2 point, out float2 normal) => (point, normal) = (Point, Normal);
    }
}