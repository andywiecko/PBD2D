using andywiecko.BurstCollections;
using andywiecko.PBD2D.Core;
using System.Linq;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace andywiecko.PBD2D.Components
{
    [RequireComponent(typeof(TriMesh))]
    [AddComponentMenu("PBD2D:TriMesh.Components/Graphics/TriMesh Renderer")]
    public class TriMeshRenderer : BaseComponent, ITriMeshRenderer
    {
        public Ref<NativeArray<float3>> MeshVertices { get; private set; }
        public NativeIndexedArray<Id<Point>, float2>.ReadOnly Positions => triMesh.Positions.Value.AsReadOnly();

        private TriMesh triMesh;
        private Mesh mesh;

        public Transform RendererTransform { get; private set; }

        private Transform TryAddRendererTransform()
        {
            var name = nameof(TriMeshRenderer);
            var rendererTransform = transform.Find(name);
            if (rendererTransform == null)
            {
                rendererTransform = new GameObject(name).transform;
                rendererTransform.SetParent(transform);
                rendererTransform.localPosition = float3.zero;
            }
            return rendererTransform;
        }

        private void RegenerateMesh()
        {
            RendererTransform = TryAddRendererTransform();
            mesh = RendererTransform.TryAddComponent<MeshFilter>().sharedMesh = new Mesh();

            triMesh = GetComponent<TriMesh>();
            var positions = triMesh.SerializedData.Positions;
            mesh.SetVertices(positions.Select(i => (Vector3)i.ToFloat3()).ToList());
            mesh.SetTriangles(triMesh.SerializedData.Triangles, submesh: 0);
            mesh.SetUVs(0, triMesh.SerializedData.UVs);

            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            mesh.RecalculateBounds();
        }

        private void OnValidate()
        {
            RendererTransform = TryAddRendererTransform();
            RendererTransform.TryAddComponent<MeshRenderer>();

            triMesh = GetComponent<TriMesh>();
            triMesh.OnSerializedDataChange += RegenerateMesh;

            RegenerateMesh();
        }

        protected override void OnDestroy()
        {
            triMesh.OnSerializedDataChange -= RegenerateMesh;

            base.OnDestroy();
        }

        private void Start()
        {
            triMesh = GetComponent<TriMesh>();
            RendererTransform = TryAddRendererTransform();
            RendererTransform.position = float3.zero;

            RegenerateMesh();

            DisposeOnDestroy(
                MeshVertices = new NativeArray<float3>(triMesh.Positions.Value.Length, Allocator.Persistent)
            );

            Redraw();
        }

        public void Redraw() { mesh.SetVertices(MeshVertices.Value); mesh.RecalculateBounds(); }

    }
}
