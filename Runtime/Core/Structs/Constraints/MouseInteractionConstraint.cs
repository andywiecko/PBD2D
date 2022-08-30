using andywiecko.BurstCollections;
using Unity.Mathematics;

namespace andywiecko.PBD2D.Core
{
    public readonly struct MouseInteractionConstraint : IPoint
    {
        public readonly Id<Point> Id { get; }
        public readonly float2 Offset;
        public MouseInteractionConstraint(Id<Point> id, float2 offset) => (Id, Offset) = (id, offset);
    }
}