using andywiecko.BurstMathUtils;
using andywiecko.ECS;
using System;
using Unity.Mathematics;
using UnityEngine;

namespace andywiecko.PBD2D.Core
{
    [Serializable]
    public class MouseInteractionConfiguration : IConfiguration
    {
        public bool IsPressed { get; set; }
        public float2 MousePosition { get; set; }
        public Complex MouseRotation { get; set; }

        [field: SerializeField, Min(0)]
        public float Radius { get; private set; } = 1;

        [field: SerializeField, Range(-1, 1)]
        public float RotationSpeed { get; private set; } = 0.1f;
    }
}