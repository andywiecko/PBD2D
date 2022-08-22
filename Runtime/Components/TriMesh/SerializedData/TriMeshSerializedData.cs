using andywiecko.PBD2D.Core;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

namespace andywiecko.PBD2D.Components
{
    public abstract class TriMeshSerializedData : ScriptableObject
    {
        [field: SerializeField, HideInInspector] public Mesh Mesh { get; protected set; } = default;
        [field: SerializeField, HideInInspector] public float2[] Positions { get; protected set; } = { };
        [field: SerializeField, HideInInspector] public int[] Triangles { get; protected set; } = { };
        [field: SerializeField, HideInInspector] public Vector2[] UVs { get; protected set; } = { };

        protected void RecalculateMesh()
        {
            Mesh.Clear();
            Mesh.SetVertices(Positions.Select(i => (Vector3)i.ToFloat3()).ToList());
            Mesh.SetTriangles(Triangles, submesh: 0);
            Mesh.SetUVs(0, UVs);
            Mesh.RecalculateBounds();
        }

        protected void SubscribeDelayedCreateMesh()
        {
            UnityEditor.EditorApplication.update += DelayedCreateMesh;
        }

        private void DelayedCreateMesh()
        {
            if (UnityEditor.EditorUtility.IsPersistent(this))
            {
                CreateMesh();
                UnityEditor.EditorApplication.update -= DelayedCreateMesh;
                return;
            }

            if (UnityEditor.Selection.activeObject != this)
            {
                UnityEditor.EditorApplication.update -= DelayedCreateMesh;
                return;
            }
        }

        private void CreateMesh()
        {
            Mesh = new() { name = $"Generated Mesh ({GetType().Name})" };
            RecalculateMesh();
            UnityEditor.AssetDatabase.AddObjectToAsset(Mesh, this);
            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.AssetDatabase.SaveAssetIfDirty(this);
        }
    }
}