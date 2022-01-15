using andywiecko.BurstCollections;
using andywiecko.PBD2D.Core;
using Unity.Mathematics;
using UnityEngine;

namespace andywiecko.PBD2D.Components
{
    [RequireComponent(typeof(TriMesh))]
    [AddComponentMenu("PBD2D:TriMesh.Components/Constraints/Triangle Area Constraint")]
    public class TriangleAreaConstraintTriMesh : BaseComponent, ITriangleAreaConstraint
    {
        public Ref<NativeIndexedArray<Id<Point>, float2>> PredictedPositions => TriMesh.PredictedPositions;
        public NativeIndexedArray<Id<Point>, float>.ReadOnly MassesInv => TriMesh.MassesInv.Value.AsReadOnly();
        public NativeIndexedArray<Id<Triangle>, Triangle>.ReadOnly Triangles => TriMesh.Triangles.Value.AsReadOnly();
        public NativeIndexedArray<Id<Triangle>, float>.ReadOnly RestAreas2 => TriMesh.RestAreas2.Value.AsReadOnly();

        [field: SerializeField, Range(0, 1)]
        public float Stiffness { get; private set; } = 1f;

        private TriMesh TriMesh { get; set; }

        private void Awake()
        {
            TriMesh = GetComponent<TriMesh>();
        }
    }
}