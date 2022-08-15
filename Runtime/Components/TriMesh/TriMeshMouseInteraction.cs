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
        public Ref<NativeReference<Id<Point>>> InteractingPointId { get; private set; }
        public Ref<NativeReference<float2>> Offset { get; private set; }

        private TriMesh triMesh;

        private void Start()
        {
            triMesh = GetComponent<TriMesh>();

            DisposeOnDestroy(
                InteractingPointId = new NativeReference<Id<Point>>(Id<Point>.Invalid, Allocator.Persistent),
                Offset = new NativeReference<float2>(Allocator.Persistent)
            );
        }
    }
}
