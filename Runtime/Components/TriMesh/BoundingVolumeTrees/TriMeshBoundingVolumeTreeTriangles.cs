using andywiecko.BurstCollections;
using andywiecko.PBD2D.Core;
using System.Linq;
using Unity.Collections;
using UnityEngine;

namespace andywiecko.PBD2D.Components
{
    [RequireComponent(typeof(TriMesh))]
    [AddComponentMenu("PBD2D:TriMesh.Components/Extended Data/Bounding Volume Tree (Triangles)")]
    public class TriMeshBoundingVolumeTreeTriangles : TriMeshBoundingVolumeTree<Triangle>
    {
        public Ref<NativeIndexedArray<Id<Triangle>, Triangle>> Triangles => triMesh.Triangles;

        private void Start()
        {
            triMesh = GetComponent<TriMesh>();

            var count = Triangles.Value.Length;
            var positions = triMesh.Positions.Value.AsReadOnly();
            var aabbs = Triangles.Value.Select(i => i.ToAABB(positions, Margin)).ToArray();
            DisposeOnDestroy(
                Volumes = new NativeIndexedArray<Id<Triangle>, AABB>(aabbs, Allocator.Persistent),
                Tree = new NativeBoundingVolumeTree<AABB>(count, Allocator.Persistent)
            );
            using var nativeAABB = new NativeArray<AABB>(aabbs, Allocator.TempJob);
            Tree.Value.Construct(nativeAABB.AsReadOnly(), default).Complete();

            volumes = Volumes.Value.GetInnerArray();
            objects = Triangles.Value.GetInnerArray();

            UpdateBounds();
        }
    }
}
