using andywiecko.PBD2D.Core;
using andywiecko.PBD2D.Systems;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace andywiecko.PBD2D.Solver
{
    public class SolverActionsGenerator
    {
        private IReadOnlyDictionary<SolverAction, List<(MethodInfo, Type)>> actionOrder;

        public SolverActionsGenerator(SolverActionsExecutionOrder actionsExecutionOrder)
        {
            actionOrder = actionsExecutionOrder.GetActionOrder();
        }

        public void Subscribe(Solver solver)
        {
            solver.ResetActions();

            foreach(var (m, t) in actionOrder[SolverAction.OnScheduling])
            {
                foreach(var s in SystemsRegistry.SystemsOf(t))
                {
                    solver.OnScheduling += () => m.Invoke(s, default);
                }
            }

            foreach (var (m, t) in actionOrder[SolverAction.OnJobsCompletion])
            {
                foreach (var s in SystemsRegistry.SystemsOf(t))
                {
                    solver.OnJobsComplete += () => m.Invoke(s, default);
                }
            }
        }
    }
}