using andywiecko.BurstCollections;
using andywiecko.PBD2D.Core;
using System.Linq;
using Unity.Collections;
using UnityEngine;

namespace andywiecko.PBD2D.Components
{
    [RequireComponent(typeof(TriMesh))]
    [RequireComponent(typeof(TriMeshExternalEdges))]
    [AddComponentMenu("PBD2D:TriMesh.Components/Extended Data/Bounding Volume Tree (External Edges)")]
    public class TriMeshBoundingVolumeTreeExternalEdges : TriMeshBoundingVolumeTree<ExternalEdge>
    {
        public Ref<NativeIndexedArray<Id<ExternalEdge>, ExternalEdge>> ExternalEdges => externalEdges.ExternalEdges;
        private TriMeshExternalEdges externalEdges;

        protected override void Awake()
        {
            base.Awake();

            triMesh = GetComponent<TriMesh>();
            externalEdges = GetComponent<TriMeshExternalEdges>();
        }

        private void Start()
        {
            var count = ExternalEdges.Value.Length;
            const Allocator Allocator = Allocator.Persistent;

            var positions = triMesh.Positions.Value.AsReadOnly();
            var edges = triMesh.Edges.Value.AsReadOnly();
            var aabbs = ExternalEdges.Value.Select(i => i.ToAABB(positions, Margin)).ToArray();
            DisposeOnDestroy(
                Volumes = new NativeIndexedArray<Id<ExternalEdge>, AABB>(aabbs, Allocator),
                Tree = new NativeBoundingVolumeTree<AABB>(count, Allocator)
            );
            using var nativeAABB = new NativeArray<AABB>(aabbs, Allocator.TempJob);
            Tree.Value.Construct(nativeAABB.AsReadOnly(), default).Complete();

            volumes = Volumes.Value.GetInnerArray();
            objects = ExternalEdges.Value.GetInnerArray();

            UpdateBounds();
        }
    }
}