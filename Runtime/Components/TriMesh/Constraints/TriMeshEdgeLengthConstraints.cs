using andywiecko.BurstCollections;
using andywiecko.ECS;
using andywiecko.PBD2D.Core;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace andywiecko.PBD2D.Components
{
    [RequireComponent(typeof(TriMesh))]
    [Category(PBDCategory.Constraints)]
    public class TriMeshEdgeLengthConstraints : BaseComponent, IEdgeLengthConstraints
    {
        public Ref<NativeIndexedArray<Id<Point>, float2>> Positions => triMesh.Positions;
        public Ref<NativeIndexedArray<Id<Point>, float>> Weights => triMesh.Weights;
        public Ref<NativeList<EdgeLengthConstraint>> Constraints { get; private set; }

        [field: SerializeField, Min(0)]
        public float Compliance { get; private set; } = 0f;

        [field: SerializeField, Range(0, 1)]
        public float Stiffness { get; private set; } = 1f;

        private TriMesh triMesh;

        private void Start()
        {
            triMesh = GetComponent<TriMesh>();

            DisposeOnDestroy(
                Constraints = new NativeList<EdgeLengthConstraint>(triMesh.Edges.Value.Length, Allocator.Persistent)
            );

            var positions = triMesh.Positions.Value.AsReadOnly();
            foreach (var e in triMesh.Edges.Value.AsReadOnly())
            {
                var l = e.GetLength(positions);
                Constraints.Value.Add(new(e, l));
            }
        }
    }
}