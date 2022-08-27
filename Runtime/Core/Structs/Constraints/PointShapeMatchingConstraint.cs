using andywiecko.BurstCollections;
using Unity.Mathematics;

namespace andywiecko.PBD2D.Core
{
    public struct PointShapeMatchingConstraint : IPoint
    {
        public readonly Id<Point> Id { get; }
        public readonly float2 InitialRelativePosition;
        public float2 RelativePosition;
        public PointShapeMatchingConstraint(Id<Point> id, float2 initialRelativePosition) =>
            (Id, InitialRelativePosition, RelativePosition) = (id, initialRelativePosition, initialRelativePosition);
        public PointShapeMatchingConstraint(Id<Point> id, float2 initialRelativePosition, float2 relativePosition) =>
            (Id, InitialRelativePosition, RelativePosition) = (id, initialRelativePosition, relativePosition);
        public static implicit operator Point(PointShapeMatchingConstraint c) => c.ToPoint();
        /// <param name="p">Relative position.</param>
        /// <param name="q">Initial relative position.</param>
        public void Deconstruct(out Id<Point> id, out float2 p, out float2 q) => (id, p, q) = (Id, RelativePosition, InitialRelativePosition);
    }
}