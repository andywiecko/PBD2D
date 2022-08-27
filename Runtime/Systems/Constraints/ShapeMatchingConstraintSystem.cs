using andywiecko.BurstCollections;
using andywiecko.BurstMathUtils;
using andywiecko.ECS;
using andywiecko.PBD2D.Core;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace andywiecko.PBD2D.Systems
{
    [Category(PBDCategory.Constraints)]
    public class ShapeMatchingConstraintSystem : BaseSystem<IShapeMatchingConstraint>
    {
        [BurstCompile]
        private struct CalculateCenterOfMassJob : IJob
        {
            [ReadOnly]
            private NativeArray<PointShapeMatchingConstraint> constraints;
            private NativeReference<float2> comRef;
            private NativeIndexedArray<Id<Point>, float>.ReadOnly weights;
            private NativeIndexedArray<Id<Point>, float2>.ReadOnly positions;
            private readonly float totalMass;

            public CalculateCenterOfMassJob(IShapeMatchingConstraint component)
            {
                constraints = component.Constraints.Value.AsDeferredJobArray();
                comRef = component.CenterOfMass;
                weights = component.Weights.Value.AsReadOnly();
                positions = component.Positions.Value.AsReadOnly();
                totalMass = component.TotalMass;
            }

            public void Execute()
            {
                comRef.Value = ShapeMatchingUtils.CalculateCenterOfMass(constraints.AsReadOnlySpan(), positions, weights, totalMass);
            }
        }

        [BurstCompile]
        private struct CalculateRelativePositionsJob : IJobParallelForDefer
        {
            private NativeArray<PointShapeMatchingConstraint> constraints;
            private NativeIndexedArray<Id<Point>, float2>.ReadOnly positions;
            private NativeReference<float2>.ReadOnly comRef;

            public CalculateRelativePositionsJob(IShapeMatchingConstraint component)
            {
                constraints = component.Constraints.Value.AsDeferredJobArray();
                positions = component.Positions.Value.AsReadOnly();
                comRef = component.CenterOfMass.Value.AsReadOnly();
            }

            public void Execute(int i)
            {
                var c = constraints[i];
                var id = c.Id;
                c.RelativePosition = positions[id] - comRef.Value;
                constraints[i] = c;
            }
        }

        [BurstCompile]
        private struct CalculateApqMatrixJob : IJob
        {
            private NativeReference<float2x2> ApqRef;
            [ReadOnly]
            private NativeArray<PointShapeMatchingConstraint> constraints;
            private NativeIndexedArray<Id<Point>, float>.ReadOnly weights;

            public CalculateApqMatrixJob(IShapeMatchingConstraint component)
            {
                ApqRef = component.ApqMatrix;
                constraints = component.Constraints.Value.AsDeferredJobArray();
                weights = component.Weights.Value.AsReadOnly();
            }

            public void Execute()
            {
                ApqRef.Value = ShapeMatchingUtils.CalculateApqMatrix(constraints.AsReadOnlySpan(), weights);
            }
        }

        [BurstCompile]
        private struct CalculateRotationMatrixJob : IJob
        {
            private NativeReference<float2x2>.ReadOnly ApqRef;
            private NativeReference<float2x2>.ReadOnly AqqRef;

            public NativeReference<Complex> RRef;
            public NativeReference<float2x2> ARef;
            private readonly float beta;

            public CalculateRotationMatrixJob(IShapeMatchingConstraint component)
            {
                ApqRef = component.ApqMatrix.Value.AsReadOnly();
                AqqRef = component.AqqMatrix.Value.AsReadOnly();
                RRef = component.Rotation;
                ARef = component.AMatrix;
                beta = component.Beta;
            }

            public void Execute()
            {
                var Apq = ApqRef.Value;
                var Aqq = AqqRef.Value;
                MathUtils.PolarDecomposition(Apq, out var V);
                RRef.Value = new Complex(V);
                var A = math.mul(Apq, Aqq);
                ARef.Value = beta * A + (1 - beta) * V;
            }
        }

        [BurstCompile]
        private struct ApplyShapeMatchingConstraintJob : IJob
        {
            private NativeIndexedArray<Id<Point>, float2> positions;
            private readonly float k;
            private NativeReference<float2x2>.ReadOnly ARef;
            [ReadOnly]
            private NativeArray<PointShapeMatchingConstraint> constraints;

            public ApplyShapeMatchingConstraintJob(IShapeMatchingConstraint component)
            {
                positions = component.Positions;
                k = component.Stiffness;
                ARef = component.AMatrix.Value.AsReadOnly();
                constraints = component.Constraints.Value.AsDeferredJobArray();
            }

            public void Execute()
            {
                foreach (var c in constraints)
                {
                    Execute(c);
                }
            }

            public void Execute(PointShapeMatchingConstraint c)
            {
                var (id, p, q) = c;
                var A = ARef.Value;
                positions[id] -= k * (p - math.mul(A, q));
            }
        }

        public override JobHandle Schedule(JobHandle dependencies = default)
        {
            foreach (var component in References)
            {
                dependencies = new CalculateCenterOfMassJob(component).Schedule(dependencies);
                dependencies = new CalculateRelativePositionsJob(component).Schedule(component.Constraints.Value, 64, dependencies);
                dependencies = new CalculateApqMatrixJob(component).Schedule(dependencies);
                dependencies = new CalculateRotationMatrixJob(component).Schedule(dependencies);
                dependencies = new ApplyShapeMatchingConstraintJob(component).Schedule(dependencies);
            }

            return dependencies;
        }
    }
}
