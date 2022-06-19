using andywiecko.ECS;
using andywiecko.PBD2D.Core;
using UnityEngine;

namespace andywiecko.PBD2D.Components
{
    [RequireComponent(typeof(TriMesh))]
    [RequireComponent(typeof(TriMeshExternalEdges))]
    [Category(PBDCategory.Collisions)]
    public class TriMeshExternalEdgesCapsuleCollider : BaseComponent
    {
        [field: SerializeField]
        public float CollisionRadius { get; private set; } = 0.1f;

        private TriMeshExternalEdges externalEdgesComponent;
        private TriMesh triMesh;

        private void Start()
        {
            triMesh = GetComponent<TriMesh>();
            externalEdgesComponent = GetComponent<TriMeshExternalEdges>();
        }

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            Gizmos.color = 0.667f * Color.green + 0.334f * Color.white;
            var externalEdges = externalEdgesComponent.ExternalEdges.Value.AsReadOnly();
            foreach (var edge in externalEdges)
            {
                var (pA, pB) = triMesh.PredictedPositions.Value.At(edge);
                GizmosExtensions.DrawCapsule(pA, pB, CollisionRadius);
            }
        }
    }
}