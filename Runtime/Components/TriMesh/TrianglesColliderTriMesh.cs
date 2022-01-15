using andywiecko.BurstCollections;
using andywiecko.PBD2D.Core;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace andywiecko.PBD2D.Components
{
    [RequireComponent(typeof(TriMesh))]
    [AddComponentMenu("PBD2D:TriMesh.Components/Collisions/Triangles Collider")]
    public class TrianglesColliderTriMesh : BaseComponent, ITrianglesColliderTriMesh
    {
        [field: SerializeField, Min(0)]
        public float Margin { get; private set; } = 0.05f;

        public Ref<NativeIndexedArray<Id<Triangle>, AABB>> AABBs { get; private set; }
        public NativeIndexedArray<Id<Point>, float2>.ReadOnly PredictedPositions => triMesh.PredictedPositions.Value.AsReadOnly();
        public NativeIndexedArray<Id<Triangle>, Triangle>.ReadOnly Triangles => triMesh.Triangles.Value.AsReadOnly();

        private TriMesh triMesh;

        private void Start()
        {
            triMesh = GetComponent<TriMesh>();

            DisposeOnDestroy(
                AABBs = new NativeIndexedArray<Id<Triangle>, AABB>(Triangles.Length, Allocator.Persistent)
            );
        }

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            Gizmos.color = Color.yellow;

            foreach (var aabb in AABBs.Value.AsReadOnly())
            {
                GizmosExtensions.DrawRectangle(aabb.Min, aabb.Max);
            }
        }
    }
}
