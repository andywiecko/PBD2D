using andywiecko.BurstCollections;
using andywiecko.ECS;
using andywiecko.PBD2D.Core;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace andywiecko.PBD2D.Components
{
    [RequireComponent(typeof(TriMesh))]
    [Category(PBDCategory.PBD)]
    public class PositionBasedDynamicsTriMesh : BaseComponent, IPositionBasedDynamics
    {
        public Ref<NativeList<Point>> Points { get; private set; }
        public Ref<NativeIndexedArray<Id<Point>, float2>> PreviousPositions => TriMesh.PreviousPositions;
        public Ref<NativeIndexedArray<Id<Point>, float2>> Positions => TriMesh.Positions;
        public Ref<NativeIndexedArray<Id<Point>, float2>> Velocities => TriMesh.Velocities;

        [field: SerializeField]
        public float2 ExternalAcceleration { get; private set; } = float2.zero;

        [field: SerializeField, Range(0, 5)]
        public float Damping { get; private set; } = 0;

        private TriMesh TriMesh { get; set; }

        private void Start()
        {
            TriMesh = GetComponent<TriMesh>();

            DisposeOnDestroy(
                Points = new NativeList<Point>(TriMesh.Points.Value.Length, Allocator.Persistent)
            );

            foreach (var i in TriMesh.Points.Value)
            {
                Points.Value.AddNoResize(i);
            }
        }
    }
}

