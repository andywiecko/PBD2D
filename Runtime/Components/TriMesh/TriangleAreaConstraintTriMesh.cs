using andywiecko.BurstCollections;
using andywiecko.PBD2D.Core;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace andywiecko.PBD2D.Components
{
    [RequireComponent(typeof(TriMesh))]
    [AddComponentMenu("PBD2D:TriMesh.Components/Constraints/Triangle Area Constraint")]
    public class TriangleAreaConstraintTriMesh : BaseComponent, ITriangleAreaConstraint
    {
        public Ref<NativeIndexedArray<Id<Point>, float2>> PredictedPositions => TriMesh.PredictedPositions;
        public Ref<NativeIndexedArray<Id<Point>, float>> MassesInv => TriMesh.MassesInv;
        public Ref<NativeList<TriangleAreaConstraint>> Constraints { get; private set; }

        [field: SerializeField, Range(0, 1)]
        public float Stiffness { get; private set; } = 1f;

        private TriMesh TriMesh { get; set; }

        private void Start()
        {
            TriMesh = GetComponent<TriMesh>();

            DisposeOnDestroy(
                Constraints = new NativeList<TriangleAreaConstraint>(TriMesh.Triangles.Value.Length, Allocator.Persistent)
            );

            foreach (var (tId, (idA, idB, idC)) in TriMesh.Triangles.Value.IdsValues)
            {
                Constraints.Value.Add(new(idA, idB, idC, TriMesh.RestAreas2.Value[tId]));
            }
        }
    }
}