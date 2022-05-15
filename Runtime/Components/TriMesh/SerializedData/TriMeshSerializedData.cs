using andywiecko.BurstTriangulator;
using andywiecko.PBD2D.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

namespace andywiecko.PBD2D.Components
{
    public abstract class TriMeshSerializedData : ScriptableObject
    {
        [Serializable]
        protected class Path : IEnumerable<float2>
        {
            [field: SerializeField]
            public float2[] Data { get; private set; } = { };
            public IEnumerator<float2> GetEnumerator() => (Data as IEnumerable<float2>).GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
            public Path(float2[] data) => Data = data;
        }

        public class ManagedInputData
        {
            public float2[] Positions;
            public int[] Constraints;
            public float2[] Holes;
        }

        [field: SerializeField, HideInInspector] public Mesh Mesh { get; protected set; } = default;
        [field: SerializeField, HideInInspector] public float[] MassesInv { get; protected set; } = { };
        [field: SerializeField, HideInInspector] public float2[] Positions { get; protected set; } = { };
        [field: SerializeField, HideInInspector] public int[] Edges { get; protected set; } = { };
        [field: SerializeField, HideInInspector] public int[] Triangles { get; protected set; } = { };
        [field: SerializeField, HideInInspector] public Vector2[] UVs { get; protected set; } = { };

        [field: SerializeField]
        public Triangulator.TriangulationSettings Settings { get; private set; } = new();

        public ManagedInputData InputData = new();

        [field: SerializeField, Min(1e-9f)]
        public float TotalMass { get; private set; } = 1;

        [SerializeField]
        protected Path[] paths = { };

        [SerializeField]
        protected float2[] holes = { };

#if UNITY_EDITOR
        public void RegenerateMesh()
        {
            if (Mesh != null)
            {
                UnityEditor.AssetDatabase.RemoveObjectFromAsset(Mesh);
            }
            Mesh = CreateMesh();
            Mesh.name = "Generated Mesh";
            UnityEditor.AssetDatabase.AddObjectToAsset(Mesh, this);

            Mesh CreateMesh()
            {
                var mesh = new Mesh();
                mesh.SetVertices(Positions.Select(i => (Vector3)i.ToFloat3()).ToList());
                mesh.SetTriangles(Triangles, submesh: 0);
                mesh.SetUVs(0, UVs);
                return mesh;
            }
        }
#endif

        private void GetPointsFromPaths()
        {
            InputData.Positions = paths.SelectMany(i => i.Data).ToArray();

            var cnstr = new List<int>();
            var offset = 0;
            foreach (var p in paths)
            {
                var n = p.Data.Length;
                cnstr.AddRange(Enumerable.Range(0, n).SelectMany(i => new[] { i + offset, (i + 1) % n + offset }).ToArray());
                offset += n;
            }

            InputData.Constraints = cnstr.ToArray();

            InputData.Holes = holes;

#if UNITY_EDITOR
            UnityEditor.EditorApplication.delayCall -= GetPointsFromPaths;
#endif 
        }

        protected virtual void OnValidate()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.delayCall += GetPointsFromPaths;
#endif
        }

        public void CopyDataFromTriangulation(Triangulator triangulator)
        {
            Triangles = triangulator.Output.Triangles.ToArray();
            Positions = triangulator.Output.Positions.ToArray();

            var pointsCount = Positions.Length;
            var trianglesCount = Triangles.Length / 3;

            var edges = new HashSet<Edge>();
            for (int i = 0; i < trianglesCount; i++)
            {
                var (a, b, c) = (Triangles[3 * i], Triangles[3 * i + 1], Triangles[3 * i + 2]);
                edges.Add((a, b));
                edges.Add((a, c));
                edges.Add((b, c));
            }

            var edgeIds = new List<int>();
            foreach (var (a, b) in edges)
            {
                edgeIds.Add((int)a);
                edgeIds.Add((int)b);
            }

            Edges = edgeIds.ToArray();

            var localMassInv = pointsCount / TotalMass;
            MassesInv = Enumerable.Repeat(localMassInv, pointsCount).ToArray();

            UpdateUVs();
        }

        protected abstract void UpdateUVs();
    }
}