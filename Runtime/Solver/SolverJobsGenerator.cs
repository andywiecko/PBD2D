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
        private readonly ISolverOptions options;

        public SolverJobsGenerator(ISolverOptions options)
        {
            this.options = options;
        }

        private class SetPhysicStateHelper
        {
            private readonly int stepId;
            private readonly int substepId;

            public SetPhysicStateHelper(int stepId, int substepId)
            {
                this.stepId = stepId;
                this.substepId = substepId;
            }

            public JobHandle Schedule(JobHandle dependencies)
            {
                PhysicsState.StepId = stepId;
                PhysicsState.SubStepId = substepId;
                return dependencies;
            }
        }

        public List<Func<JobHandle, JobHandle>> GenerateJobs()
        {
            var jobs = new List<Func<JobHandle, JobHandle>>();

            jobs.AddRange(GetJobsFor(SimulationStep.FrameStart));
            for (int step = 0; step < PhysicsState.StepCount; step++)
            {
                jobs.Add(new SetPhysicStateHelper(step, -1).Schedule);
                jobs.AddRange(GetJobsFor(SimulationStep.StepStart));
                for (int substep = 0; substep < PhysicsState.SubStepCount; substep++)
                {
                    jobs.Add(new SetPhysicStateHelper(step, substep).Schedule);
                    jobs.AddRange(GetJobsFor(SimulationStep.SubStep));
                }
                jobs.Add(new SetPhysicStateHelper(step, -1).Schedule);
                jobs.AddRange(GetJobsFor(SimulationStep.StepEnd));
            }
            jobs.Add(new SetPhysicStateHelper(-1, -1).Schedule);
            jobs.AddRange(GetJobsFor(SimulationStep.FrameEnd));

            return jobs;
        }

        private List<Func<JobHandle, JobHandle>> GetJobsFor(SimulationStep step)
        {
            var jobs = new List<Func<JobHandle, JobHandle>>();
            foreach (var type in options.SystemOrder[step])
            {
                foreach (var system in SystemsRegistry.SystemsOf(type))
                {
                    jobs.Add(system.Schedule);
                }
            }
            return jobs;
        }
    }
}