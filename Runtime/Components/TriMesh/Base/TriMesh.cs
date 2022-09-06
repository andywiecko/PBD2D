using andywiecko.BurstCollections;
using andywiecko.ECS;
using andywiecko.PBD2D.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace andywiecko.PBD2D.Components
{
    public class TriMesh : Entity
    {
        public event Action OnSerializedDataChange;

        [field: SerializeField, HideInInspector]
        public TriMeshSerializedData SerializedData { get; private set; } = default;
        private TriMeshSerializedData serializedData;

        public IPhysicalMaterial PhysicalMaterial => physicalMaterial ? physicalMaterial : Core.PhysicalMaterial.Default;
        [SerializeField]
        private PhysicalMaterial physicalMaterial = default;

        public Ref<NativeArray<Point>> Points { get; private set; }
        public Ref<NativeIndexedArray<Id<Point>, float>> Weights { get; private set; }
        public Ref<NativeIndexedArray<Id<Point>, float2>> Positions { get; private set; }
        public Ref<NativeIndexedArray<Id<Point>, float2>> PreviousPositions { get; private set; }
        public Ref<NativeIndexedArray<Id<Point>, float2>> Velocities { get; private set; }
        public Ref<NativeIndexedArray<Id<Edge>, Edge>> Edges { get; private set; }
        public Ref<NativeIndexedArray<Id<Triangle>, Triangle>> Triangles { get; private set; }

        protected override void Awake()
        {
            base.Awake();

            if (!SerializedData)
            {
                foreach (var c in GetComponents<BaseComponent>())
                {
                    c.enabled = false;
                }
                throw new NullReferenceException();
            }

            var s = transform.localScale;
            var rb = new RigidTransform(transform.rotation, transform.position);
            float2 T(float2 x) => math.transform(rb, s * (x.ToFloat3() - rb.pos) + s * rb.pos).xy;
            var transformedPositions = SerializedData.ToPositions(transformation: T);
            var pointsCount = SerializedData.Positions.Length;

            var allocator = Allocator.Persistent;
            DisposeOnDestroy(
                Points = new NativeArray<Point>(SerializedData.ToPoints(), allocator),
                Weights = new NativeIndexedArray<Id<Point>, float>(SerializedData.ToWeights(transformation: T, PhysicalMaterial.Density), allocator),
                Positions = new NativeIndexedArray<Id<Point>, float2>(transformedPositions, allocator),
                PreviousPositions = new NativeIndexedArray<Id<Point>, float2>(transformedPositions, allocator),
                Velocities = new NativeIndexedArray<Id<Point>, float2>(pointsCount, allocator),
                Edges = new NativeIndexedArray<Id<Edge>, Edge>(SerializedData.ToEdges(), allocator),
                Triangles = new NativeIndexedArray<Id<Triangle>, Triangle>(SerializedData.ToTriangles(), allocator)
            );

            transform.SetPositionAndRotation(default, quaternion.identity);
            transform.localScale = (float3)1;
        }

        private void OnValidate()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.delayCall += DelayedOnValidate;
#endif
        }

        private void DelayedOnValidate()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.delayCall -= DelayedOnValidate;
#endif
            if (SerializedData != serializedData)
            {
                OnSerializedDataChange?.Invoke();
                serializedData = SerializedData;
            }
        }

        private void OnDrawGizmos()
        {
            if (!SerializedData)
            {
                return;
            }

            var rb = new RigidTransform(transform.rotation, transform.position);
            var s = transform.localScale.ToFloat4().xyz;
            float2 T(float2 x) => Application.isPlaying ? x : math.transform(rb, s * (x.ToFloat3() - rb.pos) + s * rb.pos).xy;

            ReadOnlySpan<float2> positions = Application.isPlaying ? Positions.Value : SerializedData.Positions;
            Gizmos.color = 0.7f * Color.yellow + 0.3f * Color.green;
            foreach (var p in positions)
            {
                GizmosUtils.DrawCircle(T(p), 0.01f);
            }

            IEnumerable<(int, int, int)> triangles = Application.isPlaying ?
                Triangles.Value.Select(i => ((int)i.IdA, (int)i.IdB, (int)i.IdC)) :
                Enumerable
                    .Range(0, SerializedData.Triangles.Length / 3)
                    .Select(i =>
                    {
                        var t = SerializedData.Triangles;
                        return (t[3 * i], t[3 * i + 1], t[3 * i + 2]);
                    });

            foreach (var (a, b, c) in triangles)
            {
                var (pA, pB, pC) = (positions[a], positions[b], positions[c]);
                GizmosUtils.DrawTriangle(T(pA), T(pB), T(pC));
            }
        }
    }
}