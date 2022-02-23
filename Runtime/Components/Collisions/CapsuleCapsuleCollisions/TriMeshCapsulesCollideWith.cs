using andywiecko.BurstCollections;
using andywiecko.PBD2D.Core;
using Unity.Mathematics;

namespace andywiecko.PBD2D.Components
{
    public abstract class TriMeshCapsulesCollideWith : BaseComponent, ICapsuleCollideWithCapsule
    {
        public float CollisionRadius => triMeshCollider.CollisionRadius;
        public Ref<NativeIndexedArray<Id<Point>, float2>> PredictedPositions => triMesh.PredictedPositions;
        public Ref<NativeIndexedArray<Id<Point>, float>> MassesInv => triMesh.MassesInv;
        public Ref<NativeIndexedArray<Id<Edge>, Edge>> Edges => triMesh.Edges;
        public Ref<NativeIndexedArray<Id<CollidableEdge>, Id<Edge>>> CollidableEdges => triMeshCollider.ExternalEdgeToEdgeId;
        public Ref<NativeIndexedArray<Id<CollidableEdge>, AABB>> AABBs => triMeshCollider.AABBs.Value.RenameId<Id<ExternalEdge>, Id<CollidableEdge>, AABB>();
        public Ref<NativeIndexedArray<Id<Point>, float2>> Positions => triMesh.Positions;
        public float Friction => triMesh.PhysicalMaterial.Friction;

        private TriMesh triMesh;
        private ExternalEdgesColliderTriMesh triMeshCollider;

        private void Start()
        {
            triMesh = GetComponent<TriMesh>();
            triMeshCollider = GetComponent<ExternalEdgesColliderTriMesh>();
        }
    }
}