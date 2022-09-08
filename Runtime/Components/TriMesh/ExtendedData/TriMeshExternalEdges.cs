using andywiecko.BurstCollections;
using andywiecko.ECS;
using andywiecko.PBD2D.Core;
using Unity.Collections;
using UnityEngine;

namespace andywiecko.PBD2D.Components
{
    [RequireComponent(typeof(TriMesh))]
    [Category(PBDCategory.ExtendedData)]
    public class TriMeshExternalEdges : BaseComponent
    {
        public Ref<NativeIndexedArray<Id<ExternalEdge>, ExternalEdge>> ExternalEdges { get; private set; }

        private TriMesh triMesh;

        protected override void Awake()
        {
            base.Awake();
            triMesh = GetComponent<TriMesh>();

            DisposeOnDestroy(
                ExternalEdges = new NativeIndexedArray<Id<ExternalEdge>, ExternalEdge>(triMesh.SerializedData.ToExternalEdges(), Allocator.Persistent)
            );
        }

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            var positions = triMesh.Positions.Value.AsReadOnly();
            Gizmos.color = Color.red;
            foreach (var edge in ExternalEdges.Value.AsReadOnly())
            {
                var (pA, pB) = positions.At(edge);
                GizmosUtils.DrawLine(pA, pB);

                var p = edge.GetCenter(positions);
                var n = edge.GetNormal(positions);
                GizmosUtils.DrawRay(p, 0.33f * n);
            }
        }
    }
}