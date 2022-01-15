using andywiecko.PBD2D.Core;
using System;
using System.Collections.Generic;
using Unity.Jobs;
using UnityEngine;

namespace andywiecko.PBD2D.Solver
{
    public class Solver : MonoBehaviour
    {
        public event Action OnScheduling;
        public event Action OnJobsComplete;

        private ISolverOptions options;
        private ISolverJobsConfiguration configuration;
        private ISolverJobsGenerator jobsGenerator;
        private ISolverActionsGenerator actionsGenerator;

        private JobHandle dependencies;

        private List<Func<JobHandle, JobHandle>> jobs = new List<Func<JobHandle, JobHandle>>();

        public void ResetActions()
        {
            OnScheduling = null;
            OnJobsComplete = null;
        }

        private void Awake()
        {
            dependencies = new JobHandle();
            configuration = new SolverJobsConfiguration();
            options = configuration.Configure(new SolverOptions());
            jobsGenerator = new SolverJobsGenerator(options);
            actionsGenerator = new SolverActionsGenerator(this);

            SystemsRegistry.OnRegistryChange += RegenerateJobsList;
            SystemsRegistry.OnRegistryChange += actionsGenerator.Subscribe;
        }

        public void Start()
        {
            RegenerateJobsList();
            actionsGenerator.Subscribe();
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
            SystemsRegistry.OnRegistryChange -= actionsGenerator.Subscribe;
        }
    }
}
