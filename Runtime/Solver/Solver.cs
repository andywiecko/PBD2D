using andywiecko.PBD2D.Core;
using System;
using System.Collections.Generic;
using Unity.Jobs;
using UnityEngine;

namespace andywiecko.PBD2D.Solver
{
    public class Solver : MonoBehaviour, ISimulationConfigurationProvider
    {
        public event Action OnScheduling;
        public event Action OnJobsComplete;

        private ISolverJobsGenerator jobsGenerator;
        private SolverActionsGenerator actionsGenerator;

        private JobHandle dependencies;

        private List<Func<JobHandle, JobHandle>> jobs = new();

        [field: SerializeField]
        public SolverSystemsExecutionOrder JobsExecutionOrder { get; private set; } = default;

        [field: SerializeField]
        public SolverActionsExecutionOrder ActionsExecutionOrder { get; private set; } = default;

        [field: SerializeField]
        public SimulationConfiguration SimulationConfiguration { get; private set; } = new();

        public void ResetActions()
        {
            OnScheduling = null;
            OnJobsComplete = null;
        }

        private void Awake()
        {
            dependencies = new JobHandle();
            jobsGenerator = new SolverJobsGenerator(JobsExecutionOrder);
            actionsGenerator = new SolverActionsGenerator(ActionsExecutionOrder);

            SystemsRegistry.OnRegistryChange += RegenerateJobsList;
            SystemsRegistry.OnRegistryChange += () => actionsGenerator.Subscribe(this);
        }

        public void Start()
        {
            RegenerateJobsList();
            actionsGenerator.Subscribe(this);
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

        private void RegenerateJobsList() => jobs = jobsGenerator.GenerateJobs();

        public void OnDestroy()
        {
            SystemsRegistry.OnRegistryChange -= RegenerateJobsList;
            SystemsRegistry.OnRegistryChange -= () => actionsGenerator.Subscribe(this);
        }
    }
}
