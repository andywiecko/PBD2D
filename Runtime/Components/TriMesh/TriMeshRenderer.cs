using andywiecko.BurstCollections;
using andywiecko.ECS;
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
        public Ref<NativeIndexedArray<Id<Point>, float2>> Positions => TriMesh.Positions;

        private TriMesh TriMesh => triMesh == null ? triMesh = GetComponent<TriMesh>() : triMesh;
        private TriMesh triMesh;

        [field: SerializeField, /*HideInInspector*/]
        public Transform RendererTransform { get; set; } = default;

        private Mesh Mesh { get => meshFilter.mesh; set => meshFilter.mesh = value; }
        private MeshFilter meshFilter;

        private void Start()
        {
            RendererTransform.SetPositionAndRotation(float3.zero, quaternion.identity);
            meshFilter = RendererTransform.GetComponent<MeshFilter>();

            DisposeOnDestroy(
                MeshVertices = new NativeArray<float3>(TriMesh.Positions.Value.Length, Allocator.Persistent)
            );

            Redraw();
        }

        public void Redraw() { Mesh.SetVertices(MeshVertices.Value); Mesh.RecalculateBounds(); }
    }
}
