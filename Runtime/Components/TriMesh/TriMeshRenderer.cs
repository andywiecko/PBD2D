using andywiecko.BurstCollections;
using andywiecko.PBD2D.Core;
using Unity.Collections;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace andywiecko.PBD2D.Components
{
    [RequireComponent(typeof(TriMesh))]
    [AddComponentMenu("PBD2D:TriMesh.Components/Graphics/TriMesh Renderer")]
    public class TriMeshRenderer : BaseComponent, ITriMeshRenderer
    {
        public Ref<NativeArray<float3>> MeshVertices { get; private set; }
        public NativeIndexedArray<Id<Point>, float2>.ReadOnly Positions => TriMesh.Positions.Value.AsReadOnly();

        private TriMesh TriMesh => triMesh == null ? triMesh = GetComponent<TriMesh>() : triMesh;
        private TriMesh triMesh;

        [field: SerializeField, HideInInspector]
        public Transform RendererTransform { get; private set; } = default;

        private Mesh Mesh { get => meshFilter.mesh; set => meshFilter.mesh = value; }
        [SerializeField, HideInInspector]
        private MeshFilter meshFilter;

        private void OnValidate()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            TriMesh.OnSerializedDataChange += DelayedOnValidate;

            if (RendererTransform is null)
            {
                EditorApplication.delayCall += DelayedOnValidate;
            }
        }

        private void DelayedOnValidate()
        {
            EditorApplication.delayCall -= DelayedOnValidate;

            var newTransform = TryCreateRendererTransform();
            if (newTransform == null) return;
            RendererTransform =newTransform;
            RendererTransform.TryAddComponent<MeshRenderer>();
            meshFilter = RendererTransform.TryAddComponent<MeshFilter>();
            var mesh = GetComponent<TriMesh>().SerializedData.Mesh;
            Mesh = Instantiate(mesh);
        }

        private Transform TryCreateRendererTransform()
        {
            if (this == null || transform == null) return default;

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

        protected override void OnDestroy()
        {
            TriMesh.OnSerializedDataChange -= DelayedOnValidate;

            base.OnDestroy();
        }

        private void Start()
        {
            RendererTransform.position = float3.zero;

            DisposeOnDestroy(
                MeshVertices = new NativeArray<float3>(TriMesh.Positions.Value.Length, Allocator.Persistent)
            );

            Redraw();
        }

        public void Redraw() { Mesh.SetVertices(MeshVertices.Value); Mesh.RecalculateBounds(); }
    }
}
