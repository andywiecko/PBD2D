using andywiecko.BurstCollections;
using andywiecko.PBD2D.Core;
using System.Linq;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace andywiecko.PBD2D.Components
{
    [RequireComponent(typeof(TriMesh))]
    [RequireComponent(typeof(TriMeshExternalEdges))]
    [AddComponentMenu("PBD2D:TriMesh.Components/Extended Data/External Edge Bounding Volume Tree")]
    public class TriMeshExternalEdgeBoundingVolumeTree : BaseComponent, IExternalEdgeBoundingVolumeTree
    {
        public float Margin { get; set; } = 0.2f;
        public Ref<NativeIndexedArray<Id<Point>, float2>> Positions => triMesh.Positions;
        public Ref<BoundingVolumeTree<AABB>> Tree { get; private set; }
        public Ref<NativeIndexedArray<Id<ExternalEdge>, AABB>> AABBs { get; private set; }
        public Ref<NativeIndexedArray<Id<Edge>, Edge>> Edges => triMesh.Edges;
        public Ref<NativeIndexedArray<Id<ExternalEdge>, Id<Edge>>> ExternalEdges => externalEdges.ExternalEdges;

        private TriMesh triMesh;
        private TriMeshExternalEdges externalEdges;

        private void Awake()
        {
            triMesh = GetComponent<TriMesh>();
            externalEdges = GetComponent<TriMeshExternalEdges>();
        }

        private void Start()
        {
            var count = ExternalEdges.Value.Length;
            const Allocator Allocator = Allocator.Persistent;

            var positions = triMesh.Positions.Value.AsReadOnly();
            var edges = triMesh.Edges.Value.AsReadOnly();
            var aabbs = ExternalEdges.Value.Select(i => edges[i].ToAABB(positions, Margin)).ToArray();
            DisposeOnDestroy(
                AABBs = new NativeIndexedArray<Id<ExternalEdge>, AABB>(aabbs, Allocator),
                Tree = new BoundingVolumeTree<AABB>(count, Allocator)
            );
            using var nativeAABB = new NativeArray<AABB>(aabbs, Allocator.TempJob);
            Tree.Value.Construct(nativeAABB.AsReadOnly(), default).Complete();
        }

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            Gizmos.color = Color.yellow;
            foreach (var aabb in AABBs.Value)
            {
                GizmosExtensions.DrawRectangle(aabb.Min, aabb.Max);
            }
        }
    }
}