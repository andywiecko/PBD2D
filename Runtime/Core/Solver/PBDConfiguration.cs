using andywiecko.ECS;
using System;
using Unity.Mathematics;
using UnityEngine;

namespace andywiecko.PBD2D.Core
{
    [Serializable]
    public class PBDConfiguration : IConfiguration
    {
        /// <summary>
        /// <see cref="DeltaTime"/> devided by <see cref="StepsCount"/>, commonly marked as <em>h</em> in the literature.
        /// </summary>
        public float ReducedDeltaTime => DeltaTime / StepsCount;

        [field: SerializeField, Tooltip("Number of steps in PBD simulation.")]
        public int StepsCount { get; set; } = 2;

        [field: SerializeField, Min(1e-15f), Tooltip("The smalest time step considered in the PBD simulation step.")]
        public float DeltaTime { get; set; } = 0.001f;

        [field: SerializeField, Min(0), Tooltip("Energy dissipation factor.")]
        public float GlobalDamping { get; set; } = 0;

        [field: SerializeField, Tooltip("External acceleration applied to all PBD simulated bodies.")]
        public float2 GlobalExternalAcceleration { get; set; } = new(0, -10);
    }
}
