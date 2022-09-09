using andywiecko.PBD2D.Core;
using System.Linq;
using Unity.Mathematics;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace andywiecko.PBD2D.Components
{
    public abstract class TriMeshSerializedData : ScriptableObject
    {
        [field: SerializeField, HideInInspector] public Mesh Mesh { get; protected set; } = default;
        [field: SerializeField, HideInInspector] public float2[] Positions { get; protected set; } = { };
        [field: SerializeField, HideInInspector] public int[] Triangles { get; protected set; } = { };
        [field: SerializeField, HideInInspector] public Vector2[] UVs { get; protected set; } = { };

#if UNITY_EDITOR
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
            EditorApplication.update += DelayedCreateMesh;
        }

        private void DelayedCreateMesh()
        {
            if (EditorUtility.IsPersistent(this))
            {
                if (Mesh == null)
                {
                    CreateMesh();
                }
                EditorApplication.update -= DelayedCreateMesh;
                return;
            }

            if (Selection.activeObject != this)
            {
                EditorApplication.update -= DelayedCreateMesh;
                return;
            }
        }

        private void CreateMesh()
        {
            Mesh = new() { name = $"Generated Mesh ({GetType().Name})" };
            RecalculateMesh();
            AssetDatabase.AddObjectToAsset(Mesh, this);
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssetIfDirty(this);
        }
#endif
    }
}