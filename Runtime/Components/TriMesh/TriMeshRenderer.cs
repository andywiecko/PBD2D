using andywiecko.BurstCollections;
using andywiecko.ECS;
using andywiecko.PBD2D.Core;
using System;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace andywiecko.PBD2D.Components
{
    [RequireComponent(typeof(TriMesh))]
    [Category(PBDCategory.Graphics)]
    public class TriMeshRenderer : BaseComponent, ITriMeshRenderer
    {
        public Ref<NativeArray<float3>> MeshVertices { get; private set; }
        public Ref<NativeIndexedArray<Id<Point>, float2>> Positions => triMesh.Positions;

        [field: SerializeField, HideInInspector]
        public Transform RendererTransform { get; set; } = default;

        private TriMesh triMesh;
        private Mesh mesh;

        private void TryCreateRenderer()
        {
            if (RendererTransform != null)
            {
                return;
            }

            triMesh = GetComponent<TriMesh>();
            var name = "RendererTransform";
            var rendererTransform = transform.Find(name);
            if (rendererTransform == null)
            {
                rendererTransform = new GameObject(name).transform;
                rendererTransform.SetParent(transform);
                rendererTransform.localPosition = float3.zero;
                rendererTransform.localScale = (float3)1;
            }

            rendererTransform.TryAddComponent<MeshRenderer>();
            var filter = rendererTransform.TryAddComponent<MeshFilter>();
            filter.sharedMesh = triMesh.SerializedData.Mesh;
            RendererTransform = rendererTransform;
        }

        private void OnValidate()
        {
            EditorDelayed(TryCreateRenderer);
        }

        public void UpdateMeshReference()
        {
            TryCreateRenderer();
            var meshFilter = RendererTransform.GetComponent<MeshFilter>();
            var triMesh = GetComponent<TriMesh>();
            meshFilter.mesh = triMesh.SerializedData?.Mesh;
        }

        private void EditorDelayed(Action a)
        {
            UnityEditor.EditorApplication.delayCall += b;
            void b()
            {
                a();
                UnityEditor.EditorApplication.delayCall -= b;
            };
        }

        private void Start()
        {
            TryCreateRenderer();

            triMesh = GetComponent<TriMesh>();
            RendererTransform.SetPositionAndRotation(float3.zero, quaternion.identity);
            var meshFilter = RendererTransform.GetComponent<MeshFilter>();
            mesh = meshFilter.mesh;

            DisposeOnDestroy(
                MeshVertices = new NativeArray<float3>(triMesh.Positions.Value.Length, Allocator.Persistent)
            );

            Redraw();
        }

        public void Redraw() { mesh.SetVertices(MeshVertices.Value); mesh.RecalculateBounds(); }
    }
}
