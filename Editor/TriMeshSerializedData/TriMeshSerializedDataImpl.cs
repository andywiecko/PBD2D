using andywiecko.PBD2D.Components;
using andywiecko.PBD2D.Core;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace andywiecko.PBD2D.Editor
{
    public abstract class TriMeshSerializedDataImpl : TriMeshSerializedData
    {
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
    }
}