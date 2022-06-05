using Unity.Mathematics;
using UnityEngine;

namespace andywiecko.PBD2D.Core
{
    public class SimulationConfiguration : MonoBehaviour, IConfiguration
    {
        [field: SerializeField]
        public World World { get; private set; } = default;

        /// <summary>
        /// <see cref="DeltaTime"/> devided by <see cref="StepsCount"/>, commonly marked as <em>h</em> in the literature.
        /// </summary>
        public float ReducedDeltaTime => DeltaTime / StepsCount;

        [field: Header("Config")]

        [field: SerializeField, Tooltip("Number of steps in PBD simulation.")]
        public int StepsCount { get; set; } = 2;

        [field: SerializeField, Min(1e-15f), Tooltip("The smalest time step considered in the PBD simulation step.")]
        public float DeltaTime { get; set; } = 0.001f;

        [field: SerializeField, Tooltip("External force applied to all PBD simulated bodies.")]
        public float2 GlobalExternalForce { get; set; } = new(0, -10);

        [field: SerializeField, Min(0), Tooltip("Energy dissipation factor.")]
        public float GlobalDamping { get; set; } = 0;

        private void Awake()
        {
            World.ConfigurationsRegistry.Set(this);
        }
    }
}
