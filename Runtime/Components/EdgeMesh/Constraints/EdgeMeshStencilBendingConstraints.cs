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
    public class EdgeMeshStencilBendingConstraints : BaseComponent, IStencilBendingConstraints
    {
        [field: SerializeField, Range(0, 1)]
        public float Stiffness { get; private set; } = 1;

        [field: SerializeField, Min(0)]
        public float Compliance { get; private set; } = 0;

        public Ref<NativeIndexedArray<Id<Point>, float2>> Positions => edgeMesh.Positions;
        public Ref<NativeIndexedArray<Id<Point>, float>> Weights => edgeMesh.Weights;
        public Ref<NativeList<StencilBendingConstraint>> Constraints { get; private set; }

        private EdgeMesh edgeMesh;

        private void Start()
        {
            edgeMesh = GetComponent<EdgeMesh>();

            using var stencils = edgeMesh.SerializedData.ToStencils(Allocator.Temp);
            DisposeOnDestroy(
                Constraints = new NativeList<StencilBendingConstraint>(stencils.Length, Allocator.Persistent)
            );

            foreach (var s in stencils)
            {
                Constraints.Value.Add(StencilBendingConstraint.Create(s, edgeMesh.Positions.Value));
            }
        }
    }
}