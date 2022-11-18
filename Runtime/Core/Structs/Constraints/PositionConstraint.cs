using andywiecko.BurstCollections;
using Unity.Mathematics;

namespace andywiecko.PBD2D.Core
{
    public readonly struct PositionConstraint : IPoint
    {
        public readonly Id<Point> Id { get; }
        public readonly float2 Position { get; }
        public PositionConstraint(Id<Point> id, float2 position) => (Id, Position) = (id, position);
        public void Deconstruct(out Id<Point> id, out float2 position) => (id, position) = (Id, Position);
    }
}