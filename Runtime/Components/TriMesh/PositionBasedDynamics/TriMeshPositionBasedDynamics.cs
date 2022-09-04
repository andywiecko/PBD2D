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
    public class TriMeshPositionBasedDynamics : BaseComponent, IPositionBasedDynamics
    {
        public Ref<NativeList<Point>> Points { get; private set; }
        public Ref<NativeIndexedArray<Id<Point>, float2>> PreviousPositions => triMesh.PreviousPositions;
        public Ref<NativeIndexedArray<Id<Point>, float2>> Positions => triMesh.Positions;
        public Ref<NativeIndexedArray<Id<Point>, float2>> Velocities => triMesh.Velocities;

        [field: SerializeField]
        public float2 ExternalAcceleration { get; private set; } = float2.zero;

        [field: SerializeField, Range(0, 5)]
        public float Damping { get; private set; } = 0;

        private TriMesh triMesh;

        private void Start()
        {
            triMesh = GetComponent<TriMesh>();

            DisposeOnDestroy(
                Points = new NativeList<Point>(triMesh.Points.Value.Length, Allocator.Persistent)
            );

            foreach (var i in triMesh.Points.Value)
            {
                Points.Value.AddNoResize(i);
            }
        }
    }
}

