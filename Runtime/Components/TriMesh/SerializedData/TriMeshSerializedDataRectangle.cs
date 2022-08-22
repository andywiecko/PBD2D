using andywiecko.PBD2D.Core;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

namespace andywiecko.PBD2D.Components
{
    [PreferBinarySerialization]
    [CreateAssetMenu(
        fileName = "TriMeshSerializedDataRectangle",
        menuName = "PBD2D/TriMesh/Serialized Data (Rectangle)"
    )]
    public class TriMeshSerializedDataRectangle : TriMeshSerializedData
    {
        [SerializeField]
        private float2 size = 1;

        private void Awake()
        {
            Positions = new float2[4];
            FillPositions();

            Triangles = new[]
            {
                0, 2, 1,
                2, 0, 3
            };

            UVs = new[]
            {
                (Vector2)math.float2(0, 0),
                (Vector2)math.float2(1, 0),
                (Vector2)math.float2(1, 1),
                (Vector2)math.float2(0, 1),
            };

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
            Mesh = new();
            Mesh.name = "Generated Mesh";
            Mesh.SetVertices(Positions.Select(i => (Vector3)i.ToFloat3()).ToList());
            Mesh.SetTriangles(Triangles, submesh: 0);
            Mesh.SetUVs(0, UVs);

            UnityEditor.AssetDatabase.AddObjectToAsset(Mesh, this);
            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.AssetDatabase.SaveAssetIfDirty(this);
        }

        private void FillPositions()
        {
            Positions[0] = math.float2(0, 0);
            Positions[1] = math.float2(size.x, 0);
            Positions[2] = math.float2(size.x, size.y);
            Positions[3] = math.float2(0, size.y);
        }

        private void OnValidate()
        {
            FillPositions();
            if (Mesh != null)
            {
                Mesh.SetVertices(Positions.Select(i => (Vector3)i.ToFloat3()).ToList());
                Mesh.RecalculateBounds();
            }
        }
    }
}