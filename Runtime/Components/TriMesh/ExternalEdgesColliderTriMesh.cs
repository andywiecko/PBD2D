using andywiecko.BurstCollections;
using andywiecko.PBD2D.Core;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace andywiecko.PBD2D.Components
{
    [System.Obsolete]
    [RequireComponent(typeof(TriMesh))]
    [RequireComponent(typeof(ExternalEdgesTriMesh))]
    [AddComponentMenu("PBD2D:TriMesh.Components/Collisions/External Edges Collider")]
    public class ExternalEdgesColliderTriMesh : BaseComponent, IExternalEdgesColliderTriMesh
    {
        [field: SerializeField, Min(0)]
        public float CollisionRadius { get; private set; } = 0.1f;
        [field: SerializeField, Min(0)]
        public float Margin { get; private set; } = 0.05f;
        public Ref<NativeIndexedArray<Id<ExternalEdge>, AABB>> AABBs { get; private set; }
        public Ref<NativeIndexedArray<Id<ExternalEdge>, ExternalEdge>> ExternalEdges => externalEdgesTriMesh.ExternalEdges;
        public Ref<NativeIndexedArray<Id<Edge>, Edge>> Edges => triMesh.Edges;
        public Ref<NativeIndexedArray<Id<Point>, float2>> PredictedPositions => triMesh.PredictedPositions;

        // TODO: refactor this
        public Ref<NativeIndexedArray<Id<CollidableEdge>, Id<Edge>>> ExternalEdgeToEdgeId => externalEdgesTriMesh.ExternalEdgeToEdgeId;

        private TriMesh triMesh;
        private ExternalEdgesTriMesh externalEdgesTriMesh;

        private void Start()
        {
            triMesh = GetComponent<TriMesh>();
            externalEdgesTriMesh = GetComponent<ExternalEdgesTriMesh>();

            DisposeOnDestroy(
                AABBs = new NativeIndexedArray<Id<ExternalEdge>, AABB>(ExternalEdges.Value.Length, Allocator.Persistent)
            );
        }

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            DrawExternalEdgesCapsules();
            DrawAABBs();
        }

        private void DrawAABBs()
        {
            Gizmos.color = Color.yellow;
            foreach (var (min, max) in AABBs.Value)
            {
                GizmosExtensions.DrawRectangle(min, max);
            }
        }

        private void DrawExternalEdgesCapsules()
        {
            foreach (var externalEdge in ExternalEdges.Value)
            {
                var (idA, idB) = externalEdge;
                var pA = triMesh.PredictedPositions.Value[idA];
                var pB = triMesh.PredictedPositions.Value[idB];

                Gizmos.color = 0.667f * Color.green + 0.334f * Color.white;
                GizmosExtensions.DrawCapsule(pA, pB, CollisionRadius);
            }
        }
    }
}
