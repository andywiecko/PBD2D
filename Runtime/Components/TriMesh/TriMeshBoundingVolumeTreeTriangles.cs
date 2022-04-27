using andywiecko.BurstCollections;
using andywiecko.PBD2D.Core;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace andywiecko.PBD2D.Components
{
    [RequireComponent(typeof(TriMesh))]
    [AddComponentMenu("PBD2D:TriMesh.Components/Extended Data/Bounding Volume Tree (Triangles)")]
    public class TriMeshBoundingVolumeTreeTriangles : BaseComponent, IBoundingVolumeTreeComponent<Triangle>
    {
        public float Margin { get; set; } = 0.2f;
        public Ref<NativeBoundingVolumeTree<AABB>> Tree { get; private set; }
        public Ref<NativeIndexedArray<Id<Triangle>, AABB>> Volumes { get; private set; }
        public Ref<NativeIndexedArray<Id<Triangle>, Triangle>> Triangles => triMesh.Triangles;
        public Ref<NativeIndexedArray<Id<Point>, float2>> Positions => triMesh.Positions;

        Ref<NativeArray<AABB>> IBoundingVolumeTreeComponent<Triangle>.Volumes => volumes;
        private Ref<NativeArray<AABB>> volumes;
        Ref<NativeArray<Triangle>> IBoundingVolumeTreeComponent<Triangle>.Objects => triangles;
        private Ref<NativeArray<Triangle>> triangles;

        private TriMesh triMesh;

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
            triangles = Triangles.Value.GetInnerArray();
        }

        [SerializeField, Range(0, 30)]
        private int level = 0;

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            Gizmos.color = Color.yellow;
            foreach (var aabb in Volumes.Value)
            {
                GizmosExtensions.DrawRectangle(aabb.Min, aabb.Max);
            }

            /*
            Gizmos.color = Color.black;
            foreach(var i in 0..AABBs.Value.Length)
            {
                var aabb = Tree.Value.Volumes[i];
                GizmosExtensions.DrawRectangle(aabb.Min, aabb.Max);
            }
            */

            var tree = Tree.Value;

            var root = tree.RootId.Value;
            var set = new HashSet<int>();
            var tmpA = new Queue<int>();
            var tmpB = new Queue<int>();
            tmpA.Enqueue(root);
            for (int i = 0; i < level + 1; i++)
            {
                while (tmpA.Count > 0)
                {
                    var parent = tmpA.Dequeue();
                    if (i == level)
                        set.Add(parent);
                    var node = tree.Nodes[parent];
                    if (!node.IsLeaf)
                    {
                        tmpB.Enqueue(node.LeftChildId);
                        tmpB.Enqueue(node.RightChildId);
                    }
                    else
                    {
                        set.Add(parent);
                    }
                }
                (tmpA, tmpB) = (tmpB, tmpA);
            }

            Gizmos.color = 0.3f * Color.white + 0.7f * Color.green;
            foreach (var el in set)
            {
                var (min, max) = tree.Volumes[el];
                GizmosExtensions.DrawRectangle(min, max);
                //var (position, radius) = tree.Volumes[el];
                //Gizmos.DrawWireSphere(position.ToFloat3(), radius);
            }
        }
    }
}
