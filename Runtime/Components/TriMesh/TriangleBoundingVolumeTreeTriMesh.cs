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
    [AddComponentMenu("PBD2D:TriMesh.Components/Extended Data/Triangle Bounding Volume Tree")]
    public class TriangleBoundingVolumeTreeTriMesh : BaseComponent, ITriangleBoundingVolumeTreeTriMesh
    {
        [field: SerializeField, Min(0)]
        public float Margin { get; private set; } = 0;
        public Ref<BoundingVolumeTree<AABB>> Tree { get; private set; }
        public Ref<NativeIndexedArray<Id<Triangle>, AABB>> AABBs {get; private set;}
        public Ref<NativeIndexedArray<Id<Point>, float2>> Positions => triMesh.Positions;
        public Ref<NativeIndexedArray<Id<Triangle>, Triangle>> Triangles => triMesh.Triangles;

        [SerializeField, Range(0, 30)]
        private int level = 0;

        private TriMesh triMesh;

        private void Awake()
        {
            triMesh = GetComponent<TriMesh>();
        }

        private void Start()
        {
            var trianglesCount = triMesh.Triangles.Value.Length;
            DisposeOnDestroy(
                Tree = new BoundingVolumeTree<AABB>(leavesCount: trianglesCount, Allocator.Persistent),
                AABBs = new NativeIndexedArray<Id<Triangle>, AABB>(triMesh.Triangles.Value.Length, Allocator.Persistent)
            );
            
            var positions = triMesh.Positions.Value.AsReadOnly();
            var triangles = triMesh.Triangles.Value.AsReadOnly();
            var aabbs = triangles.ToArray().Select(i => i.ToAABB(positions)).ToArray();
            //var aabbs = triangles.ToArray().Select(i => i.ToBoundingCircle(positions)).ToArray();//ToAABB(positions).ToAABR()).ToArray();
            using var nativeAABB = new NativeArray<AABB>(aabbs, Allocator.Persistent);
            Tree.Value.Construct(nativeAABB.AsReadOnly(), default).Complete();
        }

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying)
            {
                return;
            }
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