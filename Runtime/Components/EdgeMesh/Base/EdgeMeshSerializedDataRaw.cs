using Unity.Mathematics;
using UnityEngine;

namespace andywiecko.PBD2D.Components
{
    [CreateAssetMenu(
        fileName = "EdgeMeshSerializedDataRaw",
        menuName = "PBD2D/EdgeMesh/Serialized Data (Raw)"
    )]
    public class EdgeMeshSerializedDataRaw : EdgeMeshSerializedData
    {
        public override float2[] Positions => positions;
        public override int[] Edges => edges;

        [SerializeField]
        private float2[] positions = { };

        [SerializeField]
        private int[] edges = { };
    }
}