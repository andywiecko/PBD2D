using andywiecko.BurstCollections;
using andywiecko.BurstMathUtils;
using andywiecko.PBD2D.Core;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

namespace andywiecko.PBD2D.Components
{
    [RequireComponent(typeof(ExternalEdgesTriMesh))]
    [RequireComponent(typeof(TriMesh))]
    [AddComponentMenu("PBD2D:TriMesh.Components/Extended Data/Flow Field")]
    public class FlowFieldTriMesh : BaseComponent, IFlowFieldTriMesh
    {
        // TO REMOVE
        public struct IdValueManagedEnumerator<Id, T>
        where Id : unmanaged, IIndexer where T : unmanaged
        {
            private IReadOnlyList<T> list;
            private int current;

            public IdValueManagedEnumerator(IReadOnlyList<T> list)
            {
                this.list = list;
                this.current = -1;
            }

            public static Id AsId(int value)
            {
                return UnsafeUtility.As<int, Id>(ref value);
            }

            public (Id, T) Current => (AsId(current), list[current]);
            public bool MoveNext() => ++current < list.Count;
            public IdValueManagedEnumerator<Id, T> GetEnumerator() => this;
        }


        public Ref<NativeIndexedArray<Id<Triangle>, Id<ExternalEdge>>> TrianglesEdgesField { get; private set; }

        private TriMesh triMesh;
        private ExternalEdgesTriMesh externalEdgesTriMesh;

        private NativeIndexedArray<Id<Triangle>, Triangle>.ReadOnly triangles => triMesh.Triangles.Value.AsReadOnly();
        private NativeIndexedArray<Id<Point>, float2>.ReadOnly positions => triMesh.Positions.Value.AsReadOnly();
        private NativeIndexedArray<Id<Edge>, Edge>.ReadOnly edges => triMesh.Edges.Value.AsReadOnly();
        private NativeIndexedArray<Id<ExternalEdge>, ExternalEdge>.ReadOnly externalEdges => externalEdgesTriMesh.ExternalEdges.Value.AsReadOnly();

        private IEnumerator Start()
        {
            yield return new WaitForEndOfFrame();

            triMesh = GetComponent<TriMesh>();
            externalEdgesTriMesh = GetComponent<ExternalEdgesTriMesh>();

            var allocator = Allocator.Persistent;
            DisposeOnDestroy(
                TrianglesEdgesField = new NativeIndexedArray<Id<Triangle>, Id<ExternalEdge>>(triangles.Length, allocator)
            );

            var trianglesIter = new IdValueManagedEnumerator<Id<Triangle>, Triangle>(triangles.ToArray());
            foreach (var (tId, t) in trianglesIter)
            {
                var p = t.GetCenter(positions);

                // TODO: use dbvt or some hashing.
                var minDistanceSq = float.MaxValue;
                var closestEdge = Id<ExternalEdge>.Invalid;
                var externalEdgeIter = new IdValueManagedEnumerator<Id<ExternalEdge>, ExternalEdge>(externalEdgesTriMesh.ExternalEdges.Value.AsReadOnly().ToArray());
                foreach (var (externalId, externalEdge) in externalEdgeIter)
                {
                    var (idA, idB) = externalEdge;
                    var (pA, pB) = (positions[idA], positions[idB]);
                    MathUtils.PointClosestPointOnLineSegment(p, pA, pB, out var pe);
                    var distanceSq = math.distancesq(p, pe);
                    if (distanceSq < minDistanceSq)
                    {
                        minDistanceSq = distanceSq;
                        closestEdge = externalId;
                    }
                }
                TrianglesEdgesField.Value[tId] = closestEdge;
            }
        }

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying || TrianglesEdgesField is null)
            {
                return;
            }

            Gizmos.color = 0.5f * Color.yellow + 0.5f * Color.red;

            foreach (var (tId, externalId) in TrianglesEdgesField.Value.AsReadOnly().IdsValues)
            {
                var pA = triangles[tId].GetCenter(positions);
                var edge = externalEdges[externalId].ToEdge();
                var pB = edge.GetCenter(positions);

                GizmosExtensions.DrawArrow(pA, pB);
            }
        }
    }
}
