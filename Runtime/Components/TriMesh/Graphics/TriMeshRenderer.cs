using andywiecko.BurstCollections;
using andywiecko.ECS;
using andywiecko.PBD2D.Core;
using System;
using System.Linq;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace andywiecko.PBD2D.Components
{
    [RequireComponent(typeof(TriMesh))]
    [Category(PBDCategory.Graphics)]
    public class TriMeshRenderer : BaseComponent, ITriMeshRenderer
    {
        public Mesh Mesh { get; private set; }
        public Ref<NativeArray<float3>> MeshVertices { get; private set; }
        public Ref<NativeIndexedArray<Id<Point>, float2>> Positions => triMesh.Positions;

        [SerializeField, HideInInspector]
        private Transform rendererTransform = default;

        private TriMesh triMesh;

        private void TryCreateRenderer()
        {
            if (this.rendererTransform != null)
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
            if (triMesh.SerializedData != null)
            {
                filter.sharedMesh = triMesh.SerializedData.Mesh;
            }
            this.rendererTransform = rendererTransform;
        }

        private void OnValidate()
        {
            EditorDelayed(TryCreateRenderer);
        }

        public void UpdateMeshReference()
        {
            TryCreateRenderer();
            var meshFilter = rendererTransform.GetComponent<MeshFilter>();
            var triMesh = GetComponent<TriMesh>();
            meshFilter.mesh = triMesh.SerializedData?.Mesh;
        }

        private void EditorDelayed(Action a)
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.delayCall += b;
            void b()
            {
                a();
                UnityEditor.EditorApplication.delayCall -= b;
            };
#endif
        }

        private void Start()
        {
            TryCreateRenderer();

            triMesh = GetComponent<TriMesh>();
            rendererTransform.SetPositionAndRotation(float3.zero, quaternion.identity);
            var meshFilter = rendererTransform.GetComponent<MeshFilter>();
            Mesh = meshFilter.mesh;

            var vertices = triMesh.Positions.Value.Select(i => i.ToFloat3()).ToArray();
            DisposeOnDestroy(
                MeshVertices = new NativeArray<float3>(vertices, Allocator.Persistent)
            );

            Mesh.SetVertices(MeshVertices.Value);
            Mesh.RecalculateBounds();
        }
    }
}
