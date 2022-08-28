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
        public static Point ToPoint<T>(this T point) where T : unmanaged, IPoint => new(point.Id);
        public static AABB ToAABB<T>(this T point, NativeIndexedArray<Id<Point>, float2>.ReadOnly positions, float margin = 0f)
            where T : unmanaged, IPoint
        {
            var p = positions.At(point);
            return new(min: p - margin, max: p + margin);
        }
    }

    public readonly struct Point : IPoint, IConvertableToAABB
    {
        public readonly Id<Point> Id { get; }
        public Point(Id<Point> id) => Id = id;
        public Point(int id) => Id = new(id);
        public static implicit operator Point(Id<Point> id) => new(id);
        public static implicit operator Point(int id) => new((Id<Point>)id);
        AABB IConvertableToAABB.ToAABB(NativeIndexedArray<Id<Point>, float2>.ReadOnly positions, float margin) => this.ToAABB(positions, margin);
    }
}
