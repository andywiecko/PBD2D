using Unity.Mathematics;
using UnityEngine;

namespace andywiecko.PBD2D.Editor
{
    [PreferBinarySerialization]
    [CreateAssetMenu(
        fileName = "TriMeshSerializedDataRectangle",
        menuName = "PBD2D/TriMesh/Serialized Data (Rectangle)"
    )]
    public class TriMeshSerializedDataRectangle : TriMeshSerializedDataImpl
    {
        [SerializeField]
        private float2 size = 1;

        private void Awake()
        {
            Positions = new[]
            {
                math.float2(0, 0),
                math.float2(size.x, 0),
                math.float2(size.x, size.y),
                math.float2(0, size.y)
            };

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

            SubscribeDelayedCreateMesh();
        }

        private void OnValidate()
        {
            Positions[0] = math.float2(0, 0);
            Positions[1] = math.float2(size.x, 0);
            Positions[2] = math.float2(size.x, size.y);
            Positions[3] = math.float2(0, size.y);

            RecalculateMesh();
        }
    }
}