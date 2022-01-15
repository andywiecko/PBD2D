using Unity.Mathematics;

namespace andywiecko.PBD2D.Core
{
    public readonly struct Circle
    {
        public readonly float2 Center;
        public readonly float Radius;

        public Circle(float2 center, float radius)
        {
            Center = center;
            Radius = radius;
        }

        public void Deconstruct(out float2 center, out float radius)
        {
            center = Center;
            radius = Radius;
        }

        public static implicit operator Circle((float2 p, float R) c) => new(c.p, c.R);

        public override string ToString() => $"(Circle)(Center: ({Center.x}, {Center.y}) Radius: {Radius})";
    }
}