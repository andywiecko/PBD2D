using andywiecko.BurstMathUtils;
using System;
using Unity.Mathematics;
using UnityEngine;

namespace andywiecko.PBD2D.Components
{
    [CreateAssetMenu(
        fileName = "EdgeMeshSerializedDataCircle",
        menuName = "PBD2D/EdgeMesh/Serialized Data (Circle)"
    )]
    public class EdgeMeshSerializedDataCircle : EdgeMeshSerializedData
    {
        [SerializeField, Min(3)]
        private int samples = 10;

        [SerializeField, Min(1e-9f)]
        private float radius = 1;

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
            for (int i = 0; i < p.Length; i++)
            {
                p[i] = Complex.Polar(radius, 2 * math.PI / samples * i).Value;
            }
            Positions = p;

            var e = Edges;
            Array.Resize(ref e, 2 * samples);
            for (int i = 0; i < samples - 1; i++)
            {
                e[2 * i + 0] = i + 0;
                e[2 * i + 1] = i + 1;
            }
            e[^2] = samples - 1;
            e[^1] = 0;
            Edges = e;
        }
    }
}