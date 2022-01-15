using andywiecko.PBD2D.Core;
using andywiecko.PBD2D.Systems;
using System;
using System.Collections.Generic;

namespace andywiecko.PBD2D.Solver
{
    public interface ISolverJobsConfiguration
    {
        ISolverOptions Configure(ISolverOptions options);
    }

    public interface ISolverOptions
    {
        ISystemsOrderOptions SetSystemOrder();
        public IReadOnlyDictionary<SimulationStep, List<Type>> SystemOrder { get; }
    }

    public interface ISystemsOrderOptions
    {
        ISystemsOrderOptions AtStep(SimulationStep step);
        ISystemsOrderOptions Add<T>() where T : ISystem;
        ISolverOptions ToSolverOptions();
    }

    public enum SimulationStep
    {
        Undefined = -1,
        FrameStart,
        StepStart,
        SubStep,
        StepEnd,
        FrameEnd
    }

    public class SolverOptions : ISolverOptions
    {
        public IReadOnlyDictionary<SimulationStep, List<Type>> SystemOrder => systemOrder;
        protected Dictionary<SimulationStep, List<Type>> systemOrder = new Dictionary<SimulationStep, List<Type>>();

        public ISystemsOrderOptions SetSystemOrder() => new SystemsOrderOptions(this);


        public class SystemsOrderOptions : ISystemsOrderOptions
        {
            SolverOptions options;
            SimulationStep step;

            public SystemsOrderOptions(SolverOptions options)
            {
                this.options = options;
            }

            public ISystemsOrderOptions Add<T>() where T : ISystem
            {
                options.systemOrder[step].Add(typeof(T));
                return this;
            }

            public ISystemsOrderOptions AtStep(SimulationStep step)
            {
                this.step = step;
                options.systemOrder.Add(step, new List<Type>());
                return this;
            }

            public ISolverOptions ToSolverOptions() => options;
        }
    }

    public class SolverJobsConfiguration : ISolverJobsConfiguration
    {
        public ISolverOptions Configure(ISolverOptions options) => options
            .SetSystemOrder()
                .AtStep(SimulationStep.FrameStart)
                    .Add<ExternalEdgesColliderTriMeshSystem>()
                    .Add<TrianglesColliderTriMeshSystem>()
                    .Add<TriangleBoundingVolumeTreeTriMeshSystem>()
                .AtStep(SimulationStep.StepStart)
                    .Add<PositionBasedDynamicsStepStartSystem>()
                .AtStep(SimulationStep.SubStep)
                    .Add<EdgeLengthConstraintSystem>()
                    .Add<TriangleAreaConstraintSystem>()
                    .Add<ShapeMatchingConstraintSystem>()
                    .Add<MouseInteractionSystem>()
                    .Add<CapsuleCapsuleCollisionSystem>()
                    .Add<PointLineCollisionSystem>()
                .AtStep(SimulationStep.StepEnd)
                    .Add<PositionBasedDynamicsStepEndSystem>()
                .AtStep(SimulationStep.FrameEnd)
                    .Add<TriMeshRendererSystem>()
            .ToSolverOptions();
    }
}