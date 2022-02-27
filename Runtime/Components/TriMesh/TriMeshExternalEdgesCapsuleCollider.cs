using andywiecko.PBD2D.Core;
using UnityEngine;

namespace andywiecko.PBD2D.Components
{
    [RequireComponent(typeof(TriMesh))]
    [RequireComponent(typeof(TriMeshExternalEdges))]
    [AddComponentMenu("PBD2D:TriMesh.Components/Collisions/External Edges Collider")]
    public class TriMeshExternalEdgesCapsuleCollider : BaseComponent
    {
        [field: SerializeField]
        public float CollisionRadius { get; private set; } = 0.1f;

        private void DrawExternalEdgesCapsules()
        {
            /*
            foreach (var externalEdge in ExternalEdges.Value)
            {
                var (idA, idB) = externalEdge;
                var pA = triMesh.PredictedPositions.Value[idA];
                var pB = triMesh.PredictedPositions.Value[idB];

                Gizmos.color = 0.667f * Color.green + 0.334f * Color.white;
                GizmosExtensions.DrawCapsule(pA, pB, CollisionRadius);
            }
            */
        }
    }
}