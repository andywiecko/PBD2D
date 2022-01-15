using andywiecko.PBD2D.Core;
using andywiecko.PBD2D.Systems;
using System;

namespace andywiecko.PBD2D.Solver
{
    public interface ISolverActionsGenerator
    {
        void Subscribe();
    }

    public interface IActionsGeneratorOptions
    {
        IOnSchedulingOptions OnSchedulingEvent();
        IOnJobsCompleteOptions OnJobsCompleteEvent();
    }

    public interface IOnSchedulingOptions
    {
        IOnSchedulingOptions Add<T>(Func<T, Action> func) where T : ISystem;
        IActionsGeneratorOptions ToGeneratorOptions();
    }

    public interface IOnJobsCompleteOptions
    {
        IOnJobsCompleteOptions Add<T>(Func<T, Action> func) where T : ISystem;
        IActionsGeneratorOptions ToGeneratorOptions();
    }

    public class ActionsGeneratorOptions : IActionsGeneratorOptions
    {
        private readonly Solver solver;

        public ActionsGeneratorOptions(Solver solver) => this.solver = solver;

        public IOnSchedulingOptions OnSchedulingEvent() => new OnSchedulingOptions(this);
        
        public class OnSchedulingOptions : IOnSchedulingOptions
        {
            private readonly ActionsGeneratorOptions owner;

            public OnSchedulingOptions(ActionsGeneratorOptions owner) => this.owner = owner;
            
            public IOnSchedulingOptions Add<T>(Func<T, Action> func) where T : ISystem
            {
                foreach (T system in SystemsRegistry.SystemsOf(typeof(T)))
                {
                    owner.solver.OnScheduling += func(system);
                }
                return this;
            }

            public IActionsGeneratorOptions ToGeneratorOptions() => owner;
        }

        public IOnJobsCompleteOptions OnJobsCompleteEvent() => new OnJobsCompleteOptions(this);

        public class OnJobsCompleteOptions : IOnJobsCompleteOptions
        {
            private readonly ActionsGeneratorOptions owner;

            public OnJobsCompleteOptions(ActionsGeneratorOptions owner) => this.owner = owner;

            public IOnJobsCompleteOptions Add<T>(Func<T, Action> func) where T : ISystem
            {
                foreach (T system in SystemsRegistry.SystemsOf(typeof(T)))
                {
                    owner.solver.OnJobsComplete += func(system);
                }
                return this;
            }

            public IActionsGeneratorOptions ToGeneratorOptions() => owner;
        }
    }

    public class SolverActionsGenerator : ISolverActionsGenerator
    {
        private readonly Solver solver;

        public SolverActionsGenerator(Solver solver)
        {
            this.solver = solver;
        }

        public void Subscribe()
        {
            solver.ResetActions();
            Configure(new ActionsGeneratorOptions(solver));
        }

        private IActionsGeneratorOptions Configure(IActionsGeneratorOptions options) =>
            options
                .OnSchedulingEvent()
                .ToGeneratorOptions()

                .OnJobsCompleteEvent()
                    .Add<TriMeshRendererSystem>(s => s.Redraw)
                    .Add<MouseInteractionSystem>(s => s.MouseInteractionUpdate)
                .ToGeneratorOptions();
    }
}