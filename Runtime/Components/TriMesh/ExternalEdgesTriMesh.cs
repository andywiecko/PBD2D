using andywiecko.BurstCollections;
using andywiecko.PBD2D.Core;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace andywiecko.PBD2D.Components
{
    [RequireComponent(typeof(TriMesh))]
    [AddComponentMenu("PBD2D:TriMesh.Components/Extended Data/External Edges")]
    public class ExternalEdgesTriMesh : BaseComponent, IExternalEdgesTriMesh
    {
        public Ref<NativeIndexedArray<Id<ExternalEdge>, ExternalEdge>> ExternalEdges { get; private set; }
        public Ref<NativeIndexedArray<Id<CollidableEdge>, Id<Edge>>> ExternalEdgeToEdgeId { get; private set; } // TODO: refactor this

        private TriMesh triMesh;

        private void Awake()
        {
            triMesh = GetComponent<TriMesh>();

            var edges = triMesh.SerializedData.Edges.ToEdgesArray();  //Value.AsReadOnly();
            var triangles = triMesh.SerializedData.Triangles.ToTrianglesArray();// Value.AsReadOnly();

            // TODO: cache this inside editor to draw it during editor?
            var externalEdges = new List<ExternalEdge>();
            var externalEdgeToEdge = new List<Id<Edge>>();

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
                        if (((Edge)(idA, idB)).Equals(edge))
                        {
                            externalEdges.Add(new ExternalEdge(idB, idA));
                            externalEdgeToEdge.Add(edgeId);
                        }
                    }
                }

                edgeId++;
            }

            DisposeOnDestroy(
                ExternalEdges = new NativeIndexedArray<Id<ExternalEdge>, ExternalEdge>(externalEdges.ToArray(), Allocator.Persistent),
                ExternalEdgeToEdgeId = new NativeIndexedArray<Id<CollidableEdge>, Id<Edge>>(externalEdgeToEdge.ToArray(), Allocator.Persistent)
            );
        }

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            var positions = triMesh.PredictedPositions.Value.AsReadOnly();
            foreach (var (_, externalEdge) in ExternalEdges.Value.AsReadOnly().IdsValues)
            {
                var (idA, idB) = externalEdge;
                var pA = positions[idA];
                var pB = positions[idB];

                Gizmos.color = Color.red;
                Gizmos.DrawLine(pA.ToFloat3(), pB.ToFloat3());

                var p = externalEdge.ToEdge().GetCenter(positions);
                var n = externalEdge.GetNormal(positions);
                GizmosExtensions.DrawArrow(p, p + 0.33f * n);
            }
        }
    }
}
