using andywiecko.PBD2D.Core;
using System;
using System.Collections.Generic;
using Unity.Jobs;

namespace andywiecko.PBD2D.Solver
{
    public interface ISolverJobsGenerator
    {
        List<Func<JobHandle, JobHandle>> GenerateJobs();
    }

    public class SolverJobsGenerator : ISolverJobsGenerator
    {
        private readonly ConfigurationsRegistry configurationsRegistry;
        private readonly SystemsRegistry systemsRegistry;
        private readonly IReadOnlyDictionary<SimulationStep, List<Type>> jobsOrder;

        public SolverJobsGenerator(Solver solver, ISolverJobsExecutionOrder jobsExecutionOrder)
        {
            configurationsRegistry = solver.World.ConfigurationsRegistry;
            systemsRegistry = solver.World.SystemsRegistry;
            jobsOrder = jobsExecutionOrder.GetJobsOrder();
        }

        public List<Func<JobHandle, JobHandle>> GenerateJobs()
        {
            var jobs = new List<Func<JobHandle, JobHandle>>();

            jobs.AddRange(GetJobsFor(SimulationStep.FrameStart));
            for (int step = 0; step < configurationsRegistry.Get<SimulationConfiguration>().StepsCount; step++)
            {
                jobs.AddRange(GetJobsFor(SimulationStep.Substep));
            }
            jobs.AddRange(GetJobsFor(SimulationStep.FrameEnd));

            return jobs;
        }

        private List<Func<JobHandle, JobHandle>> GetJobsFor(SimulationStep step)
        {
            var jobs = new List<Func<JobHandle, JobHandle>>();
            foreach (var type in jobsOrder[step])
            {
                var system = systemsRegistry.SystemOf(type);
                if (system != null)
                {
                    jobs.Add(system.Schedule);
                }
            }
            return jobs;
        }
    }
}