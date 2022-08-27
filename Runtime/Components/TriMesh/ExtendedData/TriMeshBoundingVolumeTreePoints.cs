using andywiecko.BurstCollections;
using andywiecko.ECS;
using andywiecko.PBD2D.Core;
using System.Linq;
using Unity.Collections;
using UnityEngine;

namespace andywiecko.PBD2D.Components
{
    [RequireComponent(typeof(TriMesh))]
    [Category(PBDCategory.ExtendedData)]
    public class TriMeshBoundingVolumeTreePoints : TriMeshBoundingVolumeTree<Point>
    {
        public Ref<NativeArray<Point>> Points => triMesh.Points;

        private void Start()
        {
            triMesh = GetComponent<TriMesh>();

            var count = Points.Value.Length;
            var positions = triMesh.Positions.Value.AsReadOnly();
            var aabbs = Points.Value.Select(i => i.ToAABB(positions, Margin)).ToArray();
            DisposeOnDestroy(
                Volumes = new NativeIndexedArray<Id<Point>, AABB>(aabbs, Allocator.Persistent),
                Tree = new NativeBoundingVolumeTree<AABB>(count, Allocator.Persistent)
            );
            using var nativeAABB = new NativeArray<AABB>(aabbs, Allocator.TempJob);
            Tree.Value.Construct(nativeAABB.AsReadOnly(), default).Complete();

            volumes = Volumes.Value.GetInnerArray();
            objects = Points.Value;

            UpdateBounds();
        }
    }
}