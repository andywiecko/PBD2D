using andywiecko.BurstCollections;
using andywiecko.ECS;
using andywiecko.PBD2D.Core;
using System;
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

        [field: SerializeField]
        public TriMeshSerializedData SerializedData = default;
        private TriMeshSerializedData serializedData;

        public IPhysicalMaterial PhysicalMaterial => physicalMaterial ? physicalMaterial : Core.PhysicalMaterial.Default;
        [SerializeField]
        private PhysicalMaterial physicalMaterial = default;

        private bool IsValid => SerializedData;

        public Ref<NativeArray<Point>> Points { get; private set; }
        public Ref<NativeIndexedArray<Id<Point>, float>> MassesInv { get; private set; }
        public Ref<NativeIndexedArray<Id<Point>, float2>> Positions { get; private set; }
        public Ref<NativeIndexedArray<Id<Point>, float2>> PreviousPositions { get; private set; }
        public Ref<NativeIndexedArray<Id<Point>, float2>> Velocities { get; private set; }
        public Ref<NativeIndexedArray<Id<Edge>, Edge>> Edges { get; private set; }
        public Ref<NativeIndexedArray<Id<Triangle>, Triangle>> Triangles { get; private set; }

        protected override void Awake()
        {
            base.Awake();

            if (!IsValid)
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
            var allocator = Allocator.Persistent;
            DisposeOnDestroy(
                Points = new NativeArray<Point>(points, allocator),
                MassesInv = new NativeIndexedArray<Id<Point>, float>(SerializedData.MassesInv, allocator),
                Positions = new NativeIndexedArray<Id<Point>, float2>(transformedPositions, allocator),
                PreviousPositions = new NativeIndexedArray<Id<Point>, float2>(transformedPositions, allocator),
                Velocities = new NativeIndexedArray<Id<Point>, float2>(SerializedData.Positions.Length, allocator),
                Edges = new NativeIndexedArray<Id<Edge>, Edge>(SerializedData.Edges.ToEdgesArray(), allocator),
                Triangles = new NativeIndexedArray<Id<Triangle>, Triangle>(SerializedData.Triangles.ToTrianglesArray(), allocator)
            );

            transform.SetPositionAndRotation(default, quaternion.identity);
            transform.localScale = (float3)1;
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

            //Gizmos.color = 0.7f * Color.blue + 0.3f * Color.green;
            Gizmos.color = 0.7f * Color.yellow + 0.3f * Color.green;

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

            if (true)
            {
                Gizmos.color = Color.blue;
                foreach (var p in Points.Value)
                {
                    var a = Positions.Value.At(p);
                    var v = Velocities.Value.At(p);

                    Gizmos.DrawRay(a.ToFloat3(), 0.001f * v.ToFloat3());
                }
            }
        }

        private void DrawPreview()
        {
            var rb = new RigidTransform(transform.rotation, transform.position);
            var s = transform.localScale.ToFloat4().xyz;
            foreach (var p in SerializedData.Positions)
            {
                Gizmos.DrawSphere(T(p.ToFloat3()), radius: 0.01f);
            }

            foreach (var (idA, idB) in SerializedData.Edges.ToEdgesArray())
            {
                var pA = SerializedData.Positions[(int)idA].ToFloat3();
                var pB = SerializedData.Positions[(int)idB].ToFloat3();
                Gizmos.DrawLine(T(pA), T(pB));
            }

            float3 T(float3 x) => math.transform(rb, s * (x - rb.pos) + s * rb.pos);
        }
    }
}