using andywiecko.BurstCollections;
using andywiecko.ECS;
using andywiecko.PBD2D.Core;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace andywiecko.PBD2D.Components
{
    [Category(PBDCategory.PBD)]
    [RequireComponent(typeof(EdgeMesh))]
    public class EdgeMeshPositionBasedDynamics : BaseComponent, IPositionBasedDynamics
    {
        public Ref<NativeList<Point>> Points { get; private set; }
        public Ref<NativeIndexedArray<Id<Point>, float2>> PreviousPositions => edgeMesh.PreviousPositions;
        public Ref<NativeIndexedArray<Id<Point>, float2>> Positions => edgeMesh.Positions;
        public Ref<NativeIndexedArray<Id<Point>, float2>> Velocities => edgeMesh.Velocities;

        [field: SerializeField]
        public float2 ExternalAcceleration { get; private set; } = float2.zero;

        [field: SerializeField, Min(0)]
        public float Damping { get; private set; } = 0;

        private EdgeMesh edgeMesh;

        private void Start()
        {
            edgeMesh = GetComponent<EdgeMesh>();

            DisposeOnDestroy(
                Points = new NativeList<Point>(edgeMesh.Points.Value.Length, Allocator.Persistent)
            );

            Points.Value.CopyFrom(edgeMesh.Points.Value);
        }
    }
}