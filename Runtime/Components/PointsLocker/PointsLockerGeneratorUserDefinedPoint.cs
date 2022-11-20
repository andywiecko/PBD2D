using andywiecko.BurstCollections;
using andywiecko.ECS;
using andywiecko.PBD2D.Core;
using Unity.Collections;
using UnityEngine;

namespace andywiecko.PBD2D.Components
{
    [Category(PBDCategory.Generator)]
    [RequireComponent(typeof(PointsLocker))]
    public class PointsLockerGeneratorUserDefinedPoint : PointsLockerGenerator
    {
        private int N => Positions.Value.Length;
        private Id<Point> PointId => new((pointId + N) % N);
        [SerializeField, Tooltip("Negative values are supported: enumeration from the end.")]
        private int pointId = default;

        protected override void Start()
        {
            base.Start();
            DisposeOnDestroy(
                Constraints = new NativeList<PositionConstraint>(1, Allocator.Persistent) { Length = 1 }
            );

            RegenerateConstraints();
            OnConstraintsGeneration();
        }

        protected override void RegenerateConstraints() => Constraints.Value[default] = GenerateConstraint();
        private PositionConstraint GenerateConstraint() => new(PointId, transform.position.ToFloat2());
    }
}