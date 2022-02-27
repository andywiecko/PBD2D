using andywiecko.BurstCollections;
using andywiecko.PBD2D.Core;
using Unity.Mathematics;
using UnityEngine;

namespace andywiecko.PBD2D.Components
{
    [RequireComponent(typeof(TriMesh))]
    [RequireComponent(typeof(TriMeshExternalEdges))]
    [RequireComponent(typeof(TriMeshExternalEdgesCapsuleCollider))]
    [RequireComponent(typeof(TriMeshExternalEdgeBoundingVolumeTree))]
    public abstract class TriMeshCapsulesCollideWith : BaseComponent, ICapsuleCollideWithCapsule
    {
        public float CollisionRadius => triMeshCollider.CollisionRadius;
        public Ref<NativeIndexedArray<Id<Point>, float2>> PredictedPositions => triMesh.PredictedPositions;
        public Ref<NativeIndexedArray<Id<Point>, float>> MassesInv => triMesh.MassesInv;
        public Ref<NativeIndexedArray<Id<Edge>, Edge>> Edges => triMesh.Edges;
        public Ref<NativeIndexedArray<Id<CollidableEdge>, Id<Edge>>> CollidableEdges => triMeshExternalEdges.ExternalEdges.Value.RenameId<Id<ExternalEdge>, Id<CollidableEdge>, Id<Edge>>();
        public Ref<NativeIndexedArray<Id<Point>, float2>> Positions => triMesh.Positions;
        public float Friction => triMesh.PhysicalMaterial.Friction;
        public Ref<BoundingVolumeTree<AABB>> Tree => externalEdgeBVT.Tree;

        private TriMesh triMesh;
        private TriMeshExternalEdges triMeshExternalEdges;
        private TriMeshExternalEdgesCapsuleCollider triMeshCollider;
        private TriMeshExternalEdgeBoundingVolumeTree externalEdgeBVT;

        private void Start()
        {
            triMesh = GetComponent<TriMesh>();
            triMeshExternalEdges = GetComponent<TriMeshExternalEdges>();
            triMeshCollider = GetComponent<TriMeshExternalEdgesCapsuleCollider>();
            externalEdgeBVT = GetComponent<TriMeshExternalEdgeBoundingVolumeTree>();
        }
    }
}