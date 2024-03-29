using andywiecko.BurstCollections;
using andywiecko.ECS;
using andywiecko.PBD2D.Core;
using Unity.Mathematics;
using UnityEngine;

namespace andywiecko.PBD2D.Components
{
    [RequireComponent(typeof(TriMeshExternalEdges))]
    [RequireComponent(typeof(TriMeshExternalEdgesCapsuleCollider))]
    [RequireComponent(typeof(TriMeshBoundingVolumeTreeExternalEdges))]
    public abstract class TriMeshCapsulesCollideWith : BaseComponent, ICapsuleCollideWithCapsule
    {
        public float CollisionRadius => triMeshCollider.CollisionRadius;
        public Ref<NativeIndexedArray<Id<Point>, float2>> Positions => triMesh.Positions;
        public Ref<NativeIndexedArray<Id<Point>, float2>> PreviousPositions => triMesh.PreviousPositions;
        public Ref<NativeIndexedArray<Id<Point>, float>> Weights => triMesh.Weights;
        public Ref<NativeIndexedArray<Id<CollidableEdge>, CollidableEdge>> CollidableEdges { get; private set; }
        public float Friction => triMesh.PhysicalMaterial.Friction;
        public Ref<NativeBoundingVolumeTree<AABB>> Tree => externalEdgeBVT.Tree;
        public AABB Bounds => externalEdgeBVT.Bounds;

        private TriMesh triMesh;
        private TriMeshExternalEdges triMeshExternalEdges;
        private TriMeshExternalEdgesCapsuleCollider triMeshCollider;
        private TriMeshBoundingVolumeTreeExternalEdges externalEdgeBVT;

        private void Start()
        {
            triMesh = GetComponent<TriMesh>();
            triMeshExternalEdges = GetComponent<TriMeshExternalEdges>();
            triMeshCollider = GetComponent<TriMeshExternalEdgesCapsuleCollider>();
            externalEdgeBVT = GetComponent<TriMeshBoundingVolumeTreeExternalEdges>();

            CollidableEdges = new(triMeshExternalEdges.ExternalEdges.Value.Reinterpret<Id<CollidableEdge>, CollidableEdge>());
        }
    }
}