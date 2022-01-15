using andywiecko.BurstCollections;
using andywiecko.PBD2D.Core;
using Unity.Mathematics;

namespace andywiecko.PBD2D.Components
{
    public abstract class TriMeshCapsulesCollideWith : BaseComponent, ICapsuleCollideWithCapsule
    {
        public float CollisionRadius => triMeshCollider.CollisionRadius;
        public Ref<NativeIndexedArray<Id<Point>, float2>> PredictedPositions => triMesh.PredictedPositions;
        public NativeIndexedArray<Id<Point>, float>.ReadOnly MassesInv => triMesh.MassesInv.Value.AsReadOnly();
        public NativeIndexedArray<Id<Edge>, Edge>.ReadOnly Edges => triMesh.Edges.Value.AsReadOnly();
        public NativeIndexedArray<Id<CollidableEdge>, Id<Edge>>.ReadOnly CollidableEdges => triMeshCollider.ExternalEdgeToEdgeId.Value.AsReadOnly();
        public NativeIndexedArray<Id<CollidableEdge>, AABB>.ReadOnly AABBs => triMeshCollider.AABBs.Value.RenameId<Id<ExternalEdge>, Id<CollidableEdge>, AABB>().AsReadOnly();

        private TriMesh triMesh;
        private ExternalEdgesColliderTriMesh triMeshCollider;

        private void Start()
        {
            triMesh = GetComponent<TriMesh>();
            triMeshCollider = GetComponent<ExternalEdgesColliderTriMesh>();
        }
    }
}