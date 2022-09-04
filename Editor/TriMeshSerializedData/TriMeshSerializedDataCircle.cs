using System.Linq;
using Unity.Mathematics;
using UnityEngine;

namespace andywiecko.PBD2D.Editor
{
    [PreferBinarySerialization]
    [CreateAssetMenu(
        fileName = "TriMeshSerializedDataCircle",
        menuName = "PBD2D/TriMesh/Serialized Data (Circle)"
    )]
    public class TriMeshSerializedDataCircle : TriMeshSerializedDataImpl
    {
        [SerializeField, Min(0)]
        private float radius = 1f;

        [SerializeField, Min(3)]
        private int samples = 10;

        private void Awake()
        {
            GenerateData();
            SubscribeDelayedCreateMesh();
        }

        private float2[] GeneratePositions()
        {
            var positions = new float2[samples + 1];

            positions[0] = 0;
            for (int i = 1; i < positions.Length; i++)
            {
                var phi = 2 * math.PI / samples * (i - 1);
                positions[i] = radius * math.float2(math.cos(phi), math.sin(phi));
            }

            return positions;
        }

        private int[] GenerateTriangles()
        {
            var triangles = new int[3 * samples];
            for (int i = 0; i < samples; i++)
            {
                triangles[3 * i + 0] = 0;
                triangles[3 * i + 1] = i == samples - 1 ? 1 : i + 2;
                triangles[3 * i + 2] = i + 1;
            }
            return triangles;
        }

        private void OnValidate()
        {
            GenerateData();
            RecalculateMesh();
        }

        private void GenerateData()
        {
            Positions = GeneratePositions();
            Triangles = GenerateTriangles();
            UVs = Positions.Select(i => (Vector2)math.clamp(0.5f * (i / radius + 1), 0, 1)).ToArray();
        }
    }
}