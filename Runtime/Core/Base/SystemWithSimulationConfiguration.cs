using System.Linq;
using UnityEngine;

namespace andywiecko.PBD2D.Core
{
    public abstract class SystemWithSimulationConfiguration<T> : BaseSystem<T> where T : IComponent
    {
        public SimulationConfiguration Configuration => ConfigurationProvider.SimulationConfiguration;
        public ISimulationConfigurationProvider ConfigurationProvider { get; set; } = default;
        [SerializeField]
        private MonoBehaviour configurationProvider = default;

        private void Awake()
        {
            ConfigurationProvider = configurationProvider as ISimulationConfigurationProvider;
        }

        protected virtual void OnValidate()
        {
            if (configurationProvider is not ISimulationConfigurationProvider)
            {
                ConfigurationProvider = default;
                configurationProvider = default;
            }

            if (configurationProvider == null)
            {
                var provider = FindObjectsOfType<MonoBehaviour>().OfType<ISimulationConfigurationProvider>().FirstOrDefault();
                if (provider is not null)
                {
                    ConfigurationProvider = provider;
                    configurationProvider = provider as MonoBehaviour;
                }
            }
        }
    }
}