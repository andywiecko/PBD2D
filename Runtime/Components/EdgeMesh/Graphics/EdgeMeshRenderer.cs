using andywiecko.BurstCollections;
using andywiecko.ECS;
using andywiecko.PBD2D.Core;
using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace andywiecko.PBD2D.Components
{
    [Category(PBDCategory.Graphics)]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(EdgeMesh))]
    public class EdgeMeshRenderer : BaseComponent, IEdgeMeshRenderer
    {
        public Ref<NativeStackedLists<Id<Point>>> Segments { get; private set; }
        public Ref<NativeStackedLists<float3>> MeshVertices { get; private set; }
        public Ref<NativeIndexedArray<Id<Point>, float2>> Positions => edgeMesh.Positions;

        public IReadOnlyList<LineRenderer> Renderers => renderers;
        [SerializeField, HideInInspector]
        private List<LineRenderer> renderers = new();

        [SerializeField, HideInInspector]
        private Transform rendererTransform = default;

        [SerializeField, HideInInspector]
        private LineRenderer defaultRendererPrefab = default;

        [SerializeField, Tooltip("If reference is empty then the default one will be used."), HideInInspector]
        private LineRenderer rendererPrefab = default;

        private EdgeMesh edgeMesh;

        private void TryCreateRenderer()
        {
            if (rendererTransform != null)
            {
                return;
            }

            edgeMesh = GetComponent<EdgeMesh>();
            var name = "RendererTransform";
            rendererTransform = transform.Find(name);

            if (rendererTransform == null)
            {
                rendererTransform = new GameObject(name).transform;
                rendererTransform.SetParent(transform);
                rendererTransform.localPosition = float3.zero;
                rendererTransform.localRotation = quaternion.identity;
                rendererTransform.localScale = (float3)1;
            }

            if (edgeMesh.SerializedData != null)
            {
                UpdateLineRenderersReferences();
            }
        }

        public void UpdateLineRenderersReferences()
        {
            if (this == null)
            {
                return;
            }

            for (int i = rendererTransform.childCount - 1; i >= 0; i--)
            {
                var r = rendererTransform.GetChild(i);
                DestroyImmediate(r.gameObject);
            }
            renderers.Clear();

            var edgeMesh = GetComponent<EdgeMesh>();
            if (edgeMesh.SerializedData == null)
            {
                return;
            }

            using var segments = edgeMesh.SerializedData.ToSegments(Allocator.Persistent);
            var prefab = rendererPrefab == null ? defaultRendererPrefab : rendererPrefab;
            foreach (var segment in segments)
            {
                var lr = Instantiate(prefab, rendererTransform);
                lr.transform.localPosition = float3.zero;
                lr.transform.localRotation = quaternion.identity;
                lr.transform.localScale = (float3)1;

                lr.name = $"Segment {{Length: {segment.Length}}}";
                lr.positionCount = segment.Length;
                lr.useWorldSpace = Application.isPlaying;
                for (int i = 0; i < segment.Length; i++)
                {
                    var id = segment[i];
                    lr.SetPosition(i, edgeMesh.SerializedData.Positions[id.Value].ToFloat3());
                }
                renderers.Add(lr);
            }
        }

        private void OnValidate() => EditorDelayed(TryCreateRenderer);

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
            edgeMesh = GetComponent<EdgeMesh>();

            DisposeOnDestroy(
                Segments = edgeMesh.SerializedData.ToSegments(Allocator.Persistent),
                MeshVertices = new NativeStackedLists<float3>(2 * edgeMesh.Points.Value.Length, Segments.Value.Length, Allocator.Persistent)
            );

            var v = MeshVertices.Value;
            foreach (var s in Segments.Value)
            {
                v.Push();
                foreach (var _ in s)
                {
                    v.Add(default);
                }
            }

            foreach (var r in Renderers)
            {
                r.useWorldSpace = true;
            }
        }
    }
}