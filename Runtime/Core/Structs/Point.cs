using andywiecko.BurstCollections;
using Unity.Mathematics;

namespace andywiecko.PBD2D.Core
{
    public interface IPoint
    {
        Id<Point> Id { get; }
    }

    public static class PointExtensions
    {
        public static Point ToPoint<T>(this T point) where T : struct, IPoint => new(point.Id);
    }

    public readonly struct Point : IPoint, IConvertableToAABB
    {
        public readonly Id<Point> Id { get; }
        public Point(Id<Point> id) => Id = id;
        public Point(int id) => Id = new(id);
        public static implicit operator Point(Id<Point> id) => new(id);
        public static implicit operator Point(int id) => new((Id<Point>)id);
        public AABB ToAABB(NativeIndexedArray<Id<Point>, float2>.ReadOnly positions, float margin = 0f)
        {
            var p = positions[Id];
            return new(min: p - margin, max: p + margin);
        }
    }
}
