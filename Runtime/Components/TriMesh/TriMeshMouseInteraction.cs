using andywiecko.BurstCollections;
using andywiecko.ECS;
using andywiecko.PBD2D.Core;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace andywiecko.PBD2D.Components
{
    [RequireComponent(typeof(TriMesh))]
    [Category(PBDCategory.Debug)]
    public class TriMeshMouseInteraction : BaseComponent, IMouseInteractionComponent
    {
        public Ref<NativeIndexedArray<Id<Point>, float2>> Positions => triMesh.Positions;
        public Ref<NativeList<MouseInteractionConstraint>> Constraints { get; private set; }

        [field: SerializeField, Range(0, 1)]
        public float Stiffness { get; private set; } = 1;

        private TriMesh triMesh;

        private void Start()
        {
            triMesh = GetComponent<TriMesh>();

            DisposeOnDestroy(
                Constraints = new NativeList<MouseInteractionConstraint>(triMesh.Points.Value.Length, Allocator.Persistent)
            );
        }
    }
}
