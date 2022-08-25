using andywiecko.BurstCollections;
using andywiecko.BurstMathUtils;
using andywiecko.ECS;
using andywiecko.PBD2D.Core;
using Unity.Collections;
using UnityEngine;

namespace andywiecko.PBD2D.Components
{
    [RequireComponent(typeof(TriMesh))]
    [RequireComponent(typeof(TriMeshExternalEdges))]
    [Category(PBDCategory.ExtendedData)]
    public class TriMeshTriField : BaseComponent
    {
        public Ref<TriFieldLookup> TriFieldLookup { get; private set; }

        [SerializeField, Range(0, 10)]
        private int samples = 4;

        private TriMeshExternalEdges externalEdges;
        private TriMesh triMesh;

        public void Start()
        {
            triMesh = GetComponent<TriMesh>();
            externalEdges = GetComponent<TriMeshExternalEdges>();

            DisposeOnDestroy(
                TriFieldLookup = new TriFieldLookup(trianglesCount: triMesh.Triangles.Value.Length, samples, Allocator.Persistent)
            );

            TriFieldLookup.Value.Initialize(
                triMesh.Positions.Value.AsReadOnly(),
                triMesh.Triangles.Value.AsReadOnly(),
                externalEdges.ExternalEdges.Value.AsReadOnly()
            ).Complete();
        }

        public void OnDrawGizmos()
        {
            if (!Application.isPlaying) return;

            var lookup = TriFieldLookup.Value;
            var positions = triMesh.Positions.Value.AsReadOnly();
            var external = externalEdges.ExternalEdges.Value.AsReadOnly();

            Gizmos.color = Color.red;
            foreach (var t in triMesh.Triangles.Value.AsReadOnly())
            {
                var (pA, pB, pC) = positions.At(t);
                foreach (var b in lookup.Barycoords)
                {
                    var p = pA * b.x + pB * b.y + pC * b.z;
                    GizmosUtils.DrawCircle(p, 0.03f);
                }
            }

            Gizmos.color = Color.blue;
            foreach (var (tId, t) in triMesh.Triangles.Value.AsReadOnly().IdsValues)
            {
                var (pA, pB, pC) = positions.At(t);
                foreach (var b in lookup.Barycoords)
                {
                    var externalId = lookup.GetExternalEdge(tId, b);
                    var edge = external[externalId];
                    var (e0, e1) = positions.At(edge);
                    var p = pA * b.x + pB * b.y + pC * b.z;
                    // TODO: this should be cached, it can be valuable!
                    MathUtils.PointClosestPointOnLineSegment(p, e0, e1, out var q);
                    GizmosUtils.DrawLine(p, q);
                }
            }
        }
    }
}