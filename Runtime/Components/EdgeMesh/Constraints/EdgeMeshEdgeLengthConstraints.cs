using andywiecko.BurstCollections;
using andywiecko.ECS;
using andywiecko.PBD2D.Core;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace andywiecko.PBD2D.Components
{
    [Category(PBDCategory.Constraints)]
    [RequireComponent(typeof(EdgeMesh))]
    public class EdgeMeshEdgeLengthConstraints : BaseComponent, IEdgeLengthConstraints
    {
        [field: SerializeField, Range(0, 1)]
        public float Stiffness { get; private set; } = 1;

        [field: SerializeField, Min(0)]
        public float Compliance { get; private set; } = 0;

        public Ref<NativeIndexedArray<Id<Point>, float2>> Positions => edgeMesh.Positions;
        public Ref<NativeIndexedArray<Id<Point>, float>> Weights => edgeMesh.Weights;
        public Ref<NativeList<EdgeLengthConstraint>> Constraints { get; private set; }

        private EdgeMesh edgeMesh;

        private void Start()
        {
            edgeMesh = GetComponent<EdgeMesh>();

            DisposeOnDestroy(
                Constraints = new NativeList<EdgeLengthConstraint>(edgeMesh.Edges.Value.Length, Allocator.Persistent)
            );

            foreach (var e in edgeMesh.Edges.Value)
            {
                Constraints.Value.AddNoResize(new(e, e.GetLength(Positions.Value)));
            }
        }
    }
}