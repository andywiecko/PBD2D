using andywiecko.BurstCollections;
using andywiecko.ECS;
using andywiecko.PBD2D.Core;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace andywiecko.PBD2D.Components
{
    [Category(PBDCategory.PBD)]
    [RequireComponent(typeof(Rod))]
    public class RodPositionBasedDynamics : BaseComponent, IPositionBasedDynamics
    {
        public Ref<NativeList<Point>> Points { get; private set; }
        public Ref<NativeIndexedArray<Id<Point>, float2>> PreviousPositions => rod.PreviousPositions;
        public Ref<NativeIndexedArray<Id<Point>, float2>> Positions => rod.Positions;
        public Ref<NativeIndexedArray<Id<Point>, float2>> Velocities => rod.Velocities;

        [field: SerializeField]
        public float2 ExternalAcceleration { get; private set; } = float2.zero;

        [field: SerializeField, Range(0, 5)]
        public float Damping { get; private set; } = 0;

        private Rod rod;

        private void Start()
        {
            rod = GetComponent<Rod>();

            DisposeOnDestroy(
                Points = new NativeList<Point>(rod.Points.Value.Length, Allocator.Persistent)
            );

            Points.Value.CopyFrom(rod.Points.Value);
        }
    }
}