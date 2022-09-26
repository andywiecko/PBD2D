using andywiecko.BurstCollections;
using andywiecko.ECS;
using andywiecko.PBD2D.Core;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace andywiecko.PBD2D.Components
{
    [RequireComponent(typeof(Rod))]
    [Category(PBDCategory.Constraints)]
    public class RodEdgeLengthConstraints : BaseComponent, IEdgeLengthConstraints
    {
        [field: SerializeField, Range(0, 1)]
        public float Stiffness { get; private set; } = 1;

        [field: SerializeField, Min(0)]
        public float Compliance { get; private set; } = 0;

        public Ref<NativeIndexedArray<Id<Point>, float2>> Positions => rod.Positions;
        public Ref<NativeIndexedArray<Id<Point>, float>> Weights => rod.Weights;
        public Ref<NativeList<EdgeLengthConstraint>> Constraints { get; private set; }

        private Rod rod;

        private void Start()
        {
            rod = GetComponent<Rod>();

            DisposeOnDestroy(
                Constraints = new NativeList<EdgeLengthConstraint>(rod.Edges.Value.Length, Allocator.Persistent)
            );

            foreach (var e in rod.Edges.Value)
            {
                Constraints.Value.AddNoResize(new(e, e.GetLength(Positions.Value)));
            }
        }
    }
}