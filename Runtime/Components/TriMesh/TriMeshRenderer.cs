using andywiecko.BurstCollections;
using andywiecko.PBD2D.Core;
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
        public NativeIndexedArray<Id<Point>, float2>.ReadOnly Positions => TriMesh.Positions.Value.AsReadOnly();

        private TriMesh TriMesh => triMesh == null ? triMesh = GetComponent<TriMesh>() : triMesh;
        private TriMesh triMesh;

        [field: SerializeField, /*HideInInspector*/]
        public Transform RendererTransform { get; set; } = default;

        private Mesh Mesh { get => meshFilter.mesh; set => meshFilter.mesh = value; }
        private MeshFilter meshFilter;

        private void Start()
        {
            RendererTransform.position = float3.zero;
            meshFilter = RendererTransform.GetComponent<MeshFilter>();

            DisposeOnDestroy(
                MeshVertices = new NativeArray<float3>(TriMesh.Positions.Value.Length, Allocator.Persistent)
            );

            Redraw();
        }

        public void Redraw() { Mesh.SetVertices(MeshVertices.Value); Mesh.RecalculateBounds(); }
    }
}
