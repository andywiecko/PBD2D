using andywiecko.BurstCollections;
using andywiecko.PBD2D.Core;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace andywiecko.PBD2D.Components
{
    [RequireComponent(typeof(TriMesh))]
    [AddComponentMenu("PBD2D:TriMesh.Components/Extended Data/External Edges")]
    public class TriMeshExternalEdges : BaseComponent
    {
        public Ref<NativeIndexedArray<Id<ExternalEdge>, Id<Edge>>> ExternalEdges { get; private set; }
        public Ref<NativeIndexedArray<Id<ExternalEdge>, float2>> ExternalNormals => throw new System.NotImplementedException();

        private TriMesh triMesh;

        protected override void Awake()
        {
            base.Awake();
            triMesh = GetComponent<TriMesh>();

            var edges = triMesh.SerializedData.Edges.ToEdgesArray();
            var triangles = triMesh.SerializedData.Triangles.ToTrianglesArray();

            // TODO: cache this inside editor to draw it during editor?
            var externalEdges = new List<Id<Edge>>();

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
                            externalEdges.Add(edgeId);
                        }
                    }
                }

                edgeId++;
            }

            DisposeOnDestroy(
                ExternalEdges = new NativeIndexedArray<Id<ExternalEdge>, Id<Edge>>(externalEdges.ToArray(), Allocator.Persistent)
            );
        }

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            Gizmos.color = Color.red;
            var positions = triMesh.PredictedPositions.Value.AsReadOnly();
            var edges = triMesh.Edges.Value.AsReadOnly();
            foreach (var (_, edgeId) in ExternalEdges.Value.AsReadOnly().IdsValues)
            {
                var edge = edges[edgeId];
                var (pA, pB) = positions.At2(edge);
                Gizmos.DrawLine(pA.ToFloat3(), pB.ToFloat3());

                /*
                var p = edge.GetCenter(positions);
                var n = math.normalizesafe(MathUtils.Rotate90CW(pB - pA));
                GizmosExtensions.DrawArrow(p, p + 0.33f * n);
                */
            }
        }
    }
}