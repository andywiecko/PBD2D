using andywiecko.PBD2D.Core;
using System;
using System.Collections.Generic;
using Unity.Jobs;
using UnityEngine;

namespace andywiecko.PBD2D.Solver
{
    public class Solver : MonoBehaviour, ISimulationConfigurationProvider
    {
        [field: SerializeField]
        public World World { get; private set; } = default;

        [field: SerializeField]
        public SolverJobsExecutionOrder JobsExecutionOrder { get; private set; } = default;

        [field: SerializeField]
        public SolverActionsExecutionOrder ActionsExecutionOrder { get; private set; } = default;

        [field: SerializeField]
        public SimulationConfiguration SimulationConfiguration { get; private set; } = new();

        public event Action OnScheduling;
        public event Action OnJobsComplete;

        private List<Func<JobHandle, JobHandle>> jobs = new();
        private ISolverJobsGenerator jobsGenerator;
        private ISolverActionsGenerator actionsGenerator;
        private JobHandle dependencies = new();

        public void ResetActions()
        {
            OnScheduling = null;
            OnJobsComplete = null;
        }

        private void Awake()
        {
            jobsGenerator = new SolverJobsGenerator(this, JobsExecutionOrder);
            actionsGenerator = new SolverActionsGenerator(this, ActionsExecutionOrder);

            World.SystemsRegistry.OnRegistryChange += RegenerateSolverTasks;
            World.Configuration = SimulationConfiguration;
        }

        public void Start()
        {
            RegenerateSolverTasks();
        }

        public void Update()
        {
            OnScheduling?.Invoke();
            ScheduleJobs().Complete();
            OnJobsComplete?.Invoke();
        }

        private JobHandle ScheduleJobs()
        {
            foreach (var job in jobs)
            {
                dependencies = job(dependencies);
            }
            return dependencies;
        }

        public void OnDestroy()
        {
            World.SystemsRegistry.OnRegistryChange -= RegenerateSolverTasks;
        }

        private void RegenerateSolverTasks()
        {
            jobs = jobsGenerator.GenerateJobs();
            actionsGenerator.GenerateActions();
        }
    }
}
