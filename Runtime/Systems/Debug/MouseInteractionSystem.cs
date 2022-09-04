using andywiecko.BurstCollections;
using andywiecko.BurstMathUtils;
using andywiecko.ECS;
using andywiecko.PBD2D.Core;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace andywiecko.PBD2D.Systems
{
    [Category(PBDCategory.Debug)]
    public class MouseInteractionSystem : BaseSystemWithConfiguration<IMouseInteractionComponent, MouseInteractionConfiguration>
    {
        private float2 MousePosition { get => Configuration.MousePosition; set => Configuration.MousePosition = value; }
        private Complex MouseRotation { get => Configuration.MouseRotation; set => Configuration.MouseRotation = value; }
        private bool IsPressed { set => Configuration.IsPressed = value; }
        private float Radius => Configuration.Radius;
        private float RotationSpeed => Configuration.RotationSpeed;

        [SolverAction]
        private void MouseInteractionUpdate()
        {
            if (!Camera.main)
            {
                return;
            }

            MousePosition = Camera.main.ScreenPointToRay(Input.mousePosition).origin.ToFloat2();
            IsPressed = Input.GetMouseButton(0);

            if (Input.GetMouseButtonDown(0))
            {
                MouseRotation = Complex.PolarUnit(0);
                foreach (var c in References)
                {
                    // TODO: use bounds
                    new GrabBodyJob(c, MousePosition, Radius).Schedule(default).Complete();
                }
            }

            if (Input.GetMouseButtonUp(0))
            {
                foreach (var c in References)
                {
                    c.Constraints.Value.Clear();
                }
            }

            var delta = Input.mouseScrollDelta;
            if (delta != default)
            {
                MouseRotation *= Complex.PolarUnit(delta.y * RotationSpeed);
            }
        }

        [BurstCompile]
        private struct GrabBodyJob : IJobParallelFor
        {
            private NativeIndexedArray<Id<Point>, float2>.ReadOnly positions;
            private NativeList<MouseInteractionConstraint>.ParallelWriter constraints;
            private readonly float2 m;
            private readonly float radiusSq;

            public GrabBodyJob(IMouseInteractionComponent component, float2 mousePosition, float radius)
            {
                positions = component.Positions.Value.AsReadOnly();
                constraints = component.Constraints.Value.AsParallelWriter();
                m = mousePosition;
                radiusSq = radius * radius;
            }

            public JobHandle Schedule(JobHandle dependencies) => this.Schedule(positions.Length, 64, dependencies);

            public void Execute(int i)
            {
                var pId = (Id<Point>)i;
                var p = positions[pId];

                if (math.distancesq(p, m) > radiusSq)
                {
                    return;
                }

                constraints.AddNoResize(new(pId, p - m));
            }
        }

        [BurstCompile]
        private struct DragBodyJob : IJobParallelForDefer
        {
            [ReadOnly]
            private NativeArray<MouseInteractionConstraint> constraints;
            [NativeDisableParallelForRestriction]
            private NativeArray<float2> positions;
            private readonly float2 mousePosition;
            private readonly Complex mouseRotation;
            private readonly float k;

            public DragBodyJob(IMouseInteractionComponent component, float2 mousePosition, Complex mouseRotation)
            {
                constraints = component.Constraints.Value.AsDeferredJobArray();
                positions = component.Positions.Value.GetInnerArray();
                this.mousePosition = mousePosition;
                this.mouseRotation = mouseRotation;
                k = component.Stiffness;
            }

            public void Execute(int i)
            {
                var c = constraints[i];
                var id = (int)c.Id;
                var p = positions[id];
                var q = mousePosition + (mouseRotation * new Complex(c.Offset)).Value;
                var n = math.normalizesafe(p - q);
                var C = math.distance(p, q);
                positions[id] -= k * n * C;
            }
        }

        public override JobHandle Schedule(JobHandle dependencies)
        {
            foreach (var component in References)
            {
                dependencies = new DragBodyJob(component, MousePosition, MouseRotation).Schedule(component.Constraints.Value, 64, dependencies);
            }
            return dependencies;
        }
    }
}
