using andywiecko.BurstCollections;
using andywiecko.ECS;
using andywiecko.PBD2D.Core;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace andywiecko.PBD2D.Components
{
    public abstract class TriMeshBoundingVolumeTree<T> : BaseComponent, IBoundingVolumeTreeComponent<T>, IBoundsComponent
        where T : struct, IConvertableToAABB
    {
        public Ref<NativeBoundingVolumeTree<AABB>> Tree { get; protected set; }
        public Ref<NativeIndexedArray<Id<T>, AABB>> Volumes { get; protected set; }
        public Ref<NativeIndexedArray<Id<Point>, float2>> Positions => triMesh.Positions;
        public AABB Bounds { get; private set; }

        [field: SerializeField, Min(0)]
        public float Margin { get; private set; } = 0.2f;

        Ref<NativeArray<AABB>> IBoundingVolumeTreeComponent<T>.Volumes => volumes;
        protected Ref<NativeArray<AABB>> volumes;
        Ref<NativeArray<T>> IBoundingVolumeTreeComponent<T>.Objects => objects;
        protected Ref<NativeArray<T>> objects;

        protected TriMesh triMesh;

        public void UpdateBounds()
        {
            var tree = Tree.Value.AsReadOnly();
            Bounds = tree.Volumes[tree.RootId.Value];
        }

        [Header("Gizmo"), SerializeField, Range(0, 30)]
        private int level = 0;

        [SerializeField]
        private Color color = 0.3f * Color.white + 0.7f * Color.green;

        protected void OnDrawGizmos()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            Gizmos.color = 0.3f * Color.red + 0.7f * Color.yellow;
            GizmosUtils.DrawRectangle(Bounds.Min, Bounds.Max);

            Gizmos.color = color;

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

            foreach (var el in set)
            {
                var (min, max) = tree.Volumes[el];
                GizmosUtils.DrawRectangle(min, max);
            }
        }
    }
}