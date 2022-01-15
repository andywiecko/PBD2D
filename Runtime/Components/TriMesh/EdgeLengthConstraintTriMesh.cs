using andywiecko.BurstCollections;
using andywiecko.PBD2D.Core;
using Unity.Mathematics;
using UnityEngine;

namespace andywiecko.PBD2D.Components
{
    [RequireComponent(typeof(TriMesh))]
    [AddComponentMenu("PBD2D:TriMesh.Components/Constraints/Edge Length Constraint")]
    public class EdgeLengthConstraintTriMesh : BaseComponent, IEdgeLengthConstraint
    {
        public Ref<NativeIndexedArray<Id<Point>, float2>> PredictedPositions => TriMesh.PredictedPositions;
        public NativeIndexedArray<Id<Point>, float>.ReadOnly MassesInv => TriMesh.MassesInv.Value.AsReadOnly();
        public NativeIndexedArray<Id<Edge>, Edge>.ReadOnly Edges => TriMesh.Edges.Value.AsReadOnly();
        public NativeIndexedArray<Id<Edge>, float>.ReadOnly RestLengths => TriMesh.RestLengths.Value.AsReadOnly();

        [field: SerializeField, Range(0, 1)]
        public float Stiffness { get; private set; } = 1f;

        private TriMesh TriMesh { get; set; }

        private void Awake()
        {
            TriMesh = GetComponent<TriMesh>();
        }
    }
}