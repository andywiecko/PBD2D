using andywiecko.BurstTriangulator;
using andywiecko.PBD2D.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace andywiecko.PBD2D.Editor
{
    [PreferBinarySerialization]
    [CreateAssetMenu(
        fileName = "TriMeshSerializedDataTexture2d",
        menuName = "PBD2D/TriMesh/Serialized Data (Texture2d)"
    )]
    public class TriMeshSerializedDataTexture2d : TriMeshSerializedDataImpl
    {
        [Serializable]
        private class SettingsWrapper
        {
            [SerializeField] int batchCount = 64;
            [SerializeField, Range(min: 0.175f, max: 0.698f), Tooltip("Angle in [rad].")] float minimumAngle = math.radians(33);
            [SerializeField, RangeMinMax(1e-9f, 2)] Vector2 area = new(0.015f, 0.5f);
            [SerializeField] bool refineMesh = true;
            [SerializeField] bool constrainEdges = true;
            [SerializeField] bool validateInput = true;
            [SerializeField] bool restoreBoundary = true;

            public static implicit operator Triangulator.TriangulationSettings(SettingsWrapper wrapper) => new()
            {
                BatchCount = wrapper.batchCount,
                MinimumAngle = wrapper.minimumAngle,
                MinimumArea = wrapper.area.x,
                MaximumArea = wrapper.area.y,
                RefineMesh = wrapper.refineMesh,
                ConstrainEdges = wrapper.constrainEdges,
                ValidateInput = wrapper.validateInput,
                RestoreBoundary = wrapper.restoreBoundary
            };
        }

        [SerializeField]
        protected Texture2D texture = default;

        [SerializeField, HideInInspector]
        protected SerializedPath path = default;

        [SerializeField]
        private SettingsWrapper settings = new();

        [SerializeField]
        private int capacity = 1024 * 1024;

        public void Triangulate()
        {
            if (texture == null)
            {
                Debug.LogWarning($"[{this}]: Missing {nameof(texture)}!", this);
                return;
            }

            if (path.Data is { Length: < 3 })
            {
                Debug.LogWarning($"[{this}]: Path should contains at least 3 points!", this);
                return;
            }

            var pointCount = path.Data.Length;
            using var positions = new NativeArray<float2>(path.Data, Allocator.Persistent);
            static IEnumerable<int> Constraints(int i, int L)
            {
                yield return i;
                yield return (i + 1) % L;
            }
            using var constraints = new NativeArray<int>(Enumerable
                .Range(0, path.Data.Length)
                .SelectMany(i => Constraints(i, pointCount))
                .ToArray(), Allocator.Persistent
            );
            using var triangulator = new Triangulator(capacity, Allocator.Persistent)
            {
                Input = new()
                {
                    Positions = positions,
                    ConstraintEdges = constraints
                },
            };

            triangulator.Settings.CopyFrom(settings);

            triangulator.Schedule(default).Complete();

            Positions = triangulator.Output.Positions.ToArray();
            Triangles = triangulator.Output.Triangles.ToArray();
            UVs = Positions
                .Select(i => (Vector2)(i / math.float2(texture.width / 100f, texture.height / 100f)))
                .ToArray();

            RecalculateMesh();
        }

        protected virtual void OnValidate()
        {
            EditorApplication.delayCall += GetPointsFromCollider;
        }

        private void GetPointsFromCollider()
        {
            if (texture != null)
            {
                var go = new GameObject();
                var renderer = go.AddComponent<SpriteRenderer>();
                var size = new Vector2(texture.width, texture.height);
                renderer.sprite = Sprite.Create(texture, new Rect(Vector2.zero, size), pivot: Vector2.zero);

                var collider = go.AddComponent<PolygonCollider2D>();
                path = new SerializedPath(collider.points.Select(i => (float2)i).ToArray());

                DestroyImmediate(go);
            }

            EditorApplication.delayCall -= GetPointsFromCollider;
        }

        private void Awake()
        {
            SubscribeDelayedCreateMesh();
        }
    }
}