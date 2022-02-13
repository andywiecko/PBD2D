using andywiecko.BurstTriangulator;
using andywiecko.PBD2D.Core;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

namespace andywiecko.PBD2D.Components
{
    [PreferBinarySerialization]
    [CreateAssetMenu(fileName = "TriMeshSerializedData (Triangulator)", menuName = "PBD2D/ScriptableObjects/TriMeshSerializedData (Triangulator)")]
    public class TriMeshSerializedDataTriangulator : TriMeshSerializedData
    {
        [field: SerializeField, HideInInspector]
        public override Mesh Mesh { get; protected set; } = default;

        [field: SerializeField]
        private Texture2D texture = default;

        [SerializeField, Min(1e-9f)]
        private float totalMass = 1;

        [field: SerializeField]
        public Vector2 AreaValues { get; set; } = default;

        [field: SerializeField]
        public float AngleValue { get; set; } = default;

        [field: SerializeField]
        public bool RefineMesh { get; private set; } = true;

        [field: SerializeField, HideInInspector]
        public override float[] MassesInv { get; protected set; } = default;
        [field: SerializeField, HideInInspector]
        public override float2[] Positions { get; protected set; } = default;

        [field: SerializeField, HideInInspector]
        public override int[] Edges { get; protected set; } = default;

        [field: SerializeField, HideInInspector]
        public override float[] RestLengths { get; protected set; } = default;

        [field: SerializeField, HideInInspector]
        public override int[] Triangles { get; protected set; } = default;

        [field: SerializeField, HideInInspector]
        public override float[] RestAreas2 { get; protected set; } = default;

        [field: SerializeField, HideInInspector]
        public override Vector2[] UVs { get; protected set; } = default;

        [field: SerializeField, HideInInspector]
        public float2[] colliderPoints { get; private set; } = default;

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

        private void GetPointsFromCollider()
        {
            if (texture != null)
            {
                var go = new GameObject();
                var renderer = go.AddComponent<SpriteRenderer>();
                var size = new Vector2(texture.width, texture.height);
                renderer.sprite = Sprite.Create(texture, new Rect(Vector2.zero, size), pivot: Vector2.zero);

                var collider = go.AddComponent<PolygonCollider2D>();

                colliderPoints = collider.points.Select(i => (float2)i).ToArray();

                DestroyImmediate(go);
            }
#if UNITY_EDITOR
            UnityEditor.EditorApplication.delayCall -= GetPointsFromCollider;
#endif 
        }

        private void OnValidate()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.delayCall += GetPointsFromCollider;
#endif
        }

        public void CopyDataFromTriangulation(Triangulator triangulator)
        {
            Triangles = triangulator.Triangles.ToArray();
            Positions = triangulator.Positions.ToArray();

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

            var edgesCount = Edges.Length / 2;

            var localMassInv = pointsCount / totalMass;
            MassesInv = Enumerable.Repeat(localMassInv, pointsCount).ToArray();
            RestLengths = Enumerable.Range(0, edgesCount).Select(i =>
            {
                var (e1, e2) = (Edges[2 * i], Edges[2 * i + 1]);
                var (pA, pB) = (Positions[e1], Positions[e2]);
                return math.distance(pA, pB);
            }).ToArray();

            RestAreas2 = Enumerable
                .Range(0, trianglesCount)
                .Select(i =>
                {
                    Triangle t = (Triangles[3 * i], Triangles[3 * i + 1], Triangles[3 * i + 2]);
                    return t.GetSignedArea2(Positions);
                }
            ).ToArray();

            UVs = Positions
                .Select(i => (Vector2)(i / math.float2(texture.width / 100f, texture.height / 100f)))
                .ToArray();
        }
    }
}