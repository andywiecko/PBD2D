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
            var transformedPositions = SerializedData.Positions
                .Select(i => T(i.ToFloat3()).xy)//  (float3)transform.TransformPoint(i.x, i.y, 0)).xy)
                .ToArray();

            float3 T(float3 x) => math.transform(rb, s * (x - rb.pos) + s * rb.pos);

            var points = Enumerable.Range(0, SerializedData.Positions.Length).Select(i => new Point((Id<Point>)i)).ToArray();

            var weights = Enumerable.Repeat(0f, points.Length).ToArray();
            var triangles = SerializedData.Triangles;
            for (int i = 0; i < triangles.Length / 3; i++)
            {
                var (a, b, c) = (triangles[3 * i], triangles[3 * i + 1], triangles[3 * i + 2]);
                var (pA, pB, pC) = (transformedPositions[a], transformedPositions[b], transformedPositions[c]);
                var area = MathUtils.TriangleSignedArea2(pA, pB, pC);
                var w0 = 6f / math.abs(area); // 3 (points) * 2 (doubled area)
                weights[a] += w0;
                weights[b] += w0;
                weights[c] += w0;
            }

            var edges = new HashSet<Edge>();
            for (int i = 0; i < triangles.Length / 3; i++)
            {
                var (a, b, c) = (triangles[3 * i], triangles[3 * i + 1], triangles[3 * i + 2]);
                edges.Add((a, b));
                edges.Add((a, c));
                edges.Add((b, c));
            }

            var allocator = Allocator.Persistent;
            DisposeOnDestroy(
                Points = new NativeArray<Point>(points, allocator),
                Weights = new NativeIndexedArray<Id<Point>, float>(weights, allocator),
                Positions = new NativeIndexedArray<Id<Point>, float2>(transformedPositions, allocator),
                PreviousPositions = new NativeIndexedArray<Id<Point>, float2>(transformedPositions, allocator),
                Velocities = new NativeIndexedArray<Id<Point>, float2>(SerializedData.Positions.Length, allocator),
                Edges = new NativeIndexedArray<Id<Edge>, Edge>(edges.ToArray(), allocator),
                Triangles = new NativeIndexedArray<Id<Triangle>, Triangle>(SerializedData.Triangles.ToTrianglesArray(), allocator)
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

            Gizmos.color = 0.7f * Color.yellow + 0.3f * Color.green;

            if (!Application.isPlaying)
            {
                DrawPreview();
                return;
            }

            foreach (var p in Positions.Value)
            {
                GizmosUtils.DrawCircle(p, 0.01f);
            }

            foreach (var edge in Edges.Value)
            {
                var (idA, idB) = edge;
                var pA = Positions.Value[idA];
                var pB = Positions.Value[idB];
                GizmosUtils.DrawLine(pA, pB);
            }

            if (true)
            {
                Gizmos.color = Color.blue;
                foreach (var p in Points.Value)
                {
                    var a = Positions.Value.At(p);
                    var v = Velocities.Value.At(p);

                    GizmosUtils.DrawRay(a, 0.001f * v);
                }
            }
        }

        private void DrawPreview()
        {
            var rb = new RigidTransform(transform.rotation, transform.position);
            var s = transform.localScale.ToFloat4().xyz;
            foreach (var p in SerializedData.Positions)
            {
                GizmosUtils.DrawCircle(T(p.ToFloat3()), 0.01f);
            }

            var triangles = SerializedData.Triangles;
            var positions = SerializedData.Positions;
            for (int i = 0; i < triangles.Length / 3; i++)
            {
                var (a, b, c) = (triangles[3 * i], triangles[3 * i + 1], triangles[3 * i + 2]);
                var (pA, pB, pC) = (positions[a].ToFloat3(), positions[b].ToFloat3(), positions[c].ToFloat3());
                GizmosUtils.DrawLine(T(pA), T(pB));
                GizmosUtils.DrawLine(T(pB), T(pC));
                GizmosUtils.DrawLine(T(pC), T(pA));
            }

            float2 T(float3 x) => math.transform(rb, s * (x - rb.pos) + s * rb.pos).xy;
        }
    }
}