using System;
using UnityEngine;

namespace andywiecko.PBD2D.Components
{
    [CreateAssetMenu(
        fileName = "EdgeMeshSerializedDataLine",
        menuName = "PBD2D/EdgeMesh/Serialized Data (Line)"
    )]
    public class EdgeMeshSerializedDataLine : EdgeMeshSerializedData
    {
        [SerializeField, Min(2)]
        private int samples = 10;

        [SerializeField, Min(1e-9f)]
        private float length = 1;

        private void Awake()
        {
            GeneratePoints();
        }

        private void OnValidate()
        {
            GeneratePoints();
        }

        private void GeneratePoints()
        {
            var p = Positions;
            Array.Resize(ref p, samples);
            var dx = length / (samples - 1);
            for (int i = 0; i < p.Length; i++)
            {
                p[i] = new(i * dx, 0);
            }
            Positions = p;

            var e = Edges;
            Array.Resize(ref e, 2 * (samples - 1));
            for (int i = 0; i < samples - 1; i++)
            {
                e[2 * i + 0] = i + 0;
                e[2 * i + 1] = i + 1;
            }
            Edges = e;
        }
    }
}