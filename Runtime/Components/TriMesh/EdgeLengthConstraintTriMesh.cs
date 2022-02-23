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
        public Ref<NativeIndexedArray<Id<Point>, float>> MassesInv => TriMesh.MassesInv;
        public Ref<NativeIndexedArray<Id<Edge>, Edge>> Edges => TriMesh.Edges;
        public Ref<NativeIndexedArray<Id<Edge>, float>> RestLengths => TriMesh.RestLengths;

        [field: SerializeField, Range(0, 1)]
        public float Stiffness { get; private set; } = 1f;

        private TriMesh TriMesh { get; set; }

        private void Awake()
        {
            TriMesh = GetComponent<TriMesh>();
        }
    }
}