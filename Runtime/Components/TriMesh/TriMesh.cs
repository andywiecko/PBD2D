using andywiecko.BurstCollections;
using andywiecko.BurstMathUtils;
using andywiecko.ECS;
using andywiecko.PBD2D.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Collections.NotBurstCompatible;
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
                throw new NullReferenceException();
            }

            var s = transform.localScale;
            var rb = new RigidTransform(transform.rotation, transform.position);
            var transformedPositions = SerializedData.Positions.Select(i => T(i)).ToArray();

            float2 T(float2 x) => math.transform(rb, s * (x.ToFloat3() - rb.pos) + s * rb.pos).xy;

            var pointsCount = SerializedData.Positions.Length;
            var points = Enumerable.Range(0, pointsCount).Select(i => new Point((Id<Point>)i)).ToArray();
            var triangles = SerializedData.ToTrianglesArray();
            var edges = triangles.SelectMany(i => unpack(i)).ToArray();
            var weights = new float[pointsCount];
            var rho = PhysicalMaterial.Density;
            foreach (var (a, b, c) in triangles)
            {
                var (pA, pB, pC) = (transformedPositions[(int)a], transformedPositions[(int)b], transformedPositions[(int)c]);
                var area = MathUtils.TriangleSignedArea2(pA, pB, pC);
                var w0 = 6f / rho / math.abs(area); // 3 (points) * 2 (doubled area)
                weights[(int)a] += w0;
                weights[(int)b] += w0;
                weights[(int)c] += w0;
            }

            static IEnumerable<Edge> unpack(Triangle t)
            {
                var (a, b, c) = t;
                yield return new(a, b);
                yield return new(a, c);
                yield return new(b, c);
            }

            var allocator = Allocator.Persistent;
            DisposeOnDestroy(
                Points = new NativeArray<Point>(points, allocator),
                Weights = new NativeIndexedArray<Id<Point>, float>(weights, allocator),
                Positions = new NativeIndexedArray<Id<Point>, float2>(transformedPositions, allocator),
                PreviousPositions = new NativeIndexedArray<Id<Point>, float2>(transformedPositions, allocator),
                Velocities = new NativeIndexedArray<Id<Point>, float2>(pointsCount, allocator),
                Edges = new NativeIndexedArray<Id<Edge>, Edge>(edges, allocator),
                Triangles = new NativeIndexedArray<Id<Triangle>, Triangle>(triangles, allocator)
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