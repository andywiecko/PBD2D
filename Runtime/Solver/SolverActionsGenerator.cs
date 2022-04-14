using andywiecko.PBD2D.Core;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace andywiecko.PBD2D.Solver
{
    public interface ISolverActionsGenerator
    {
        void GenerateActions();
    }

    public class SolverActionsGenerator : ISolverActionsGenerator
    {
        private readonly IReadOnlyDictionary<SolverAction, List<(MethodInfo, Type)>> actionsOrder;
        private readonly Solver solver;

        public SolverActionsGenerator(Solver solver, SolverActionsExecutionOrder actionsExecutionOrder)
        {
            this.solver = solver;
            actionsOrder = actionsExecutionOrder.GetActionsOrder();
        }

        public void GenerateActions()
        {
            solver.ResetActions();

            foreach (var (method, type) in actionsOrder[SolverAction.OnScheduling])
            {
                foreach (var system in solver.World.SystemsRegistry.SystemsOf(type))
                {
                    solver.OnScheduling += () => method.Invoke(system, default);
                }
            }

            foreach (var (method, type) in actionsOrder[SolverAction.OnJobsCompletion])
            {
                foreach (var system in solver.World.SystemsRegistry.SystemsOf(type))
                {
                    solver.OnJobsComplete += () => method.Invoke(system, default);
                }
            }
        }
    }
}