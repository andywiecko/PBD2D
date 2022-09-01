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
    public class TriMeshTriangleAreaConstraints : BaseComponent, ITriangleAreaConstraints
    {
        public Ref<NativeIndexedArray<Id<Point>, float2>> Positions => triMesh.Positions;
        public Ref<NativeIndexedArray<Id<Point>, float>> Weights => triMesh.Weights;
        public Ref<NativeList<TriangleAreaConstraint>> Constraints { get; private set; }

        [field: SerializeField, Min(0)]
        public float Compliance { get; private set; } = 0;

        [field: SerializeField, Range(0, 1)]
        public float Stiffness { get; private set; } = 1f;

        private TriMesh triMesh;

        private void Start()
        {
            triMesh = GetComponent<TriMesh>();

            DisposeOnDestroy(
                Constraints = new NativeList<TriangleAreaConstraint>(triMesh.Triangles.Value.Length, Allocator.Persistent)
            );

            var positions = triMesh.Positions.Value.AsReadOnly();
            foreach (var t in triMesh.Triangles.Value.AsReadOnly())
            {
                var a2 = t.GetSignedArea2(positions);
                Constraints.Value.Add(new(t, a2));
            }
        }
    }
}