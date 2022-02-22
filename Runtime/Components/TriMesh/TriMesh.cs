using andywiecko.BurstCollections;
using andywiecko.PBD2D.Core;
using System;
using System.Linq;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace andywiecko.PBD2D.Components
{
    public class TriMesh : BaseComponent
    {
        public event Action OnSerializedDataChange;

        [field: SerializeField]
        public TriMeshSerializedData SerializedData = default;
        private TriMeshSerializedData serializedData;

        public IPhysicalMaterial PhysicalMaterial => physicalMaterial ? physicalMaterial : Core.PhysicalMaterial.Default;
        [SerializeField]
        private PhysicalMaterial physicalMaterial = default;

        private bool IsValid => SerializedData;

        public Ref<NativeIndexedArray<Id<Point>, float>> MassesInv { get; private set; }
        public Ref<NativeIndexedArray<Id<Point>, float2>> Positions { get; private set; }
        public Ref<NativeIndexedArray<Id<Point>, float2>> PredictedPositions { get; private set; }
        public Ref<NativeIndexedArray<Id<Point>, float2>> Velocities { get; private set; }
        public Ref<NativeIndexedArray<Id<Edge>, Edge>> Edges { get; private set; }
        public Ref<NativeIndexedArray<Id<Edge>, float>> RestLengths { get; private set; }
        public Ref<NativeIndexedArray<Id<Triangle>, Triangle>> Triangles { get; private set; }
        public Ref<NativeIndexedArray<Id<Triangle>, float>> RestAreas2 { get; private set; }

        private void Awake()
        {
            if (!IsValid)
            {
                throw new NullReferenceException();
            }

            var transformedPositions = SerializedData.Positions
                .Select(i => ((float3)transform.TransformPoint(i.x, i.y, 0)).xy)
                .ToArray();

            var allocator = Allocator.Persistent;
            DisposeOnDestroy(
                MassesInv = new NativeIndexedArray<Id<Point>, float>(SerializedData.MassesInv, allocator),
                Positions = new NativeIndexedArray<Id<Point>, float2>(transformedPositions, allocator),
                PredictedPositions = new NativeIndexedArray<Id<Point>, float2>(transformedPositions, allocator),
                Velocities = new NativeIndexedArray<Id<Point>, float2>(Enumerable.Repeat(float2.zero, SerializedData.Positions.Length).ToArray(), allocator),
                Edges = new NativeIndexedArray<Id<Edge>, Edge>(SerializedData.Edges.ToEdgesArray(), allocator),
                RestLengths = new NativeIndexedArray<Id<Edge>, float>(SerializedData.RestLengths, allocator),
                Triangles = new NativeIndexedArray<Id<Triangle>, Triangle>(SerializedData.Triangles.ToTrianglesArray(), allocator),
                RestAreas2 = new NativeIndexedArray<Id<Triangle>, float>(SerializedData.RestAreas2, allocator)
            );
        }

        private void OnValidate()
        {
            if (!IsValid)
            {
                return;
            }
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
            if (!IsValid)
            {
                return;
            }


            Gizmos.color = 0.7f * Color.blue + 0.3f * Color.green;

            if (!Application.isPlaying)
            {
                DrawPreview();
                return;
            }

            foreach (var position in Positions.Value)
            {
                Gizmos.DrawSphere(position.ToFloat3(), radius: 0.01f);
            }

            foreach (var edge in Edges.Value)
            {
                var (idA, idB) = edge;
                var pA = Positions.Value[idA];
                var pB = Positions.Value[idB];
                Gizmos.DrawLine(pA.ToFloat3(), pB.ToFloat3());
            }
        }

        private void DrawPreview()
        {
            float3 offset = transform.position;
            foreach (var p in SerializedData.Positions)
            {
                Gizmos.DrawSphere(offset + p.ToFloat3(), radius: 0.01f);
            }

            foreach (var (idA, idB) in SerializedData.Edges.ToEdgesArray())
            {
                var pA = offset + SerializedData.Positions[(int)idA].ToFloat3();
                var pB = offset + SerializedData.Positions[(int)idB].ToFloat3();
                Gizmos.DrawLine(pA, pB);
            }
        }
    }
}