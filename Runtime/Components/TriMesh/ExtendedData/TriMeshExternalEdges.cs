using andywiecko.BurstCollections;
using andywiecko.ECS;
using andywiecko.PBD2D.Core;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;

namespace andywiecko.PBD2D.Components
{
    [RequireComponent(typeof(TriMesh))]
    [Category(PBDCategory.ExtendedData)]
    public class TriMeshExternalEdges : BaseComponent
    {
        public Ref<NativeIndexedArray<Id<ExternalEdge>, ExternalEdge>> ExternalEdges { get; private set; }
        // TODO: store normals?

        private TriMesh triMesh;

        protected override void Awake()
        {
            base.Awake();
            triMesh = GetComponent<TriMesh>();

            var triangles = triMesh.SerializedData.Triangles.ToTrianglesArray();
            var edgesSet = new HashSet<Edge>();
            foreach (var (a, b, c) in triangles)
            {
                edgesSet.Add((a, b));
                edgesSet.Add((b, c));
                edgesSet.Add((c, a));
            }
            var edges = edgesSet.ToArray();

            // TODO: cache this inside editor to draw it during editor?
            var externalEdges = new List<ExternalEdge>();

            var edgeId = Id<Edge>.Zero;
            foreach (var edge in edges)
            {
                var tmp = new FixedList32Bytes<Triangle>();
                foreach (var triangle in triangles)
                {
                    if (triangle.Contains(edge))
                    {
                        tmp.Add(triangle);
                    }
                }

                if (tmp.Length == 1)
                {
                    var triangle = tmp[0];
                    for (int j = 0, i = 2; j < 3; i = j++)
                    {
                        var idA = triangle[i];
                        var idB = triangle[j];
                        if (((Edge)(idA, idB)).Equals(edge) || ((Edge)(idB, idA)).Equals(edge))
                        {
                            externalEdges.Add(new(idA, idB));
                        }
                    }
                }

                edgeId++;
            }

            DisposeOnDestroy(
                ExternalEdges = new NativeIndexedArray<Id<ExternalEdge>, ExternalEdge>(externalEdges.ToArray(), Allocator.Persistent)
            );
        }

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            Gizmos.color = Color.red;
            var positions = triMesh.Positions.Value.AsReadOnly();
            var edges = triMesh.Edges.Value.AsReadOnly();
            foreach (var edge in ExternalEdges.Value.AsReadOnly())
            {
                var (pA, pB) = positions.At(edge);
                Gizmos.DrawLine(pA.ToFloat3(), pB.ToFloat3());

                var p = edge.GetCenter(positions);
                var n = edge.GetNormal(positions);
                GizmosExtensions.DrawArrow(p, p + 0.33f * n);
            }
        }
    }
}