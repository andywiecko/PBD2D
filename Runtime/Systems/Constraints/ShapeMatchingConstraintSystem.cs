using andywiecko.BurstCollections;
using andywiecko.BurstMathUtils;
using andywiecko.PBD2D.Core;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace andywiecko.PBD2D.Systems
{
    [AddComponentMenu("PBD2D:Systems/Constraints/Shape Matching Constraint System")]
    public class ShapeMatchingConstraintSystem : BaseSystem<IShapeMatchingConstraint>
    {
        [BurstCompile]
        private struct CalculateCenterOfMassJob : IJob
        {
            private NativeReference<float2> com;
            private NativeIndexedArray<Id<Point>, float>.ReadOnly massesInv;
            private NativeIndexedArray<Id<Point>, float2>.ReadOnly positions;
            private readonly float M;

            public CalculateCenterOfMassJob(IShapeMatchingConstraint component)
            {
                com = component.CenterOfMass;
                massesInv = component.MassesInv;
                positions = component.PredictedPositions.Value.AsReadOnly();
                M = component.TotalMass;
            }

            public void Execute()
            {
                com.Value = ShapeMatchingUtils.CalculateCenterOfMass(positions, massesInv, totalMass: M);
            }
        }

        [BurstCompile]
        private struct CalculateRelativePositionsJob : IJobParallelFor
        {
            private NativeIndexedArray<Id<Point>, float2>.ReadOnly positions;
            private NativeIndexedArray<Id<Point>, float2> relativePositions;
            private NativeReference<float2>.ReadOnly com;

            public CalculateRelativePositionsJob(IShapeMatchingConstraint component)
            {
                positions = component.PredictedPositions.Value.AsReadOnly();
                relativePositions = component.RelativePositions;
                com = component.CenterOfMass.Value.AsReadOnly();
            }

            public JobHandle Schedule(JobHandle dependencies)
            {
                return this.Schedule(relativePositions.Length, innerloopBatchCount: 64, dependencies);
            }

            public void Execute(int i)
            {
                var pId = (Id<Point>)i;
                relativePositions[pId] = positions[pId] - com.Value;
            }
        }

        [BurstCompile]
        private struct CalculateApqMatrixJob : IJob
        {
            private NativeReference<float2x2> Apq;
            private NativeIndexedArray<Id<Point>, float2>.ReadOnly relativePositions;
            private NativeIndexedArray<Id<Point>, float2>.ReadOnly initialRelativePositions;
            private NativeIndexedArray<Id<Point>, float>.ReadOnly massesInv;

            public CalculateApqMatrixJob(IShapeMatchingConstraint component)
            {
                Apq = component.ApqMatrix;
                relativePositions = component.RelativePositions.Value.AsReadOnly();
                initialRelativePositions = component.InitialRelativePositions;
                massesInv = component.MassesInv;
            }

            public void Execute()
            {
                var apq = float2x2.zero;
                foreach (var (pId, p) in relativePositions.IdsValues)
                {
                    var m = 1 / massesInv[pId];
                    var q = initialRelativePositions[pId];
                    apq += m * MathUtils.OuterProduct(p, q);
                }

                Apq.Value = apq;
            }
        }

        [BurstCompile]
        private struct CalculateRotationMatrixJob : IJob
        {
            private NativeReference<float2x2>.ReadOnly ApqRef;
            private readonly float2x2 Aqq;

            public NativeReference<Complex> RRef;
            public NativeReference<float2x2> ARef;
            private readonly float beta;

            public CalculateRotationMatrixJob(IShapeMatchingConstraint component)
            {
                ApqRef = component.ApqMatrix.Value.AsReadOnly();
                Aqq = component.AqqMatrix;
                RRef = component.Rotation;
                ARef = component.AMatrix;
                beta = component.Beta;
            }

            public void Execute()
            {
                var Apq = ApqRef.Value;
                MathUtils.PolarDecomposition(Apq, out var V);
                RRef.Value = new Complex(V);
                var A = math.mul(Apq, Aqq);
                ARef.Value = beta * A + (1 - beta) * V;
            }
        }

        [BurstCompile]
        private struct ApplyShapeMatchingConstraintJob : IJobParallelFor
        {
            private NativeIndexedArray<Id<Point>, float2> positions;
            private readonly float k;
            private NativeReference<float2x2>.ReadOnly ARef;
            private NativeIndexedArray<Id<Point>, float2>.ReadOnly initialRelativePositions;
            private NativeReference<float2>.ReadOnly comRef;

            public ApplyShapeMatchingConstraintJob(IShapeMatchingConstraint component)
            {
                positions = component.PredictedPositions;
                k = component.Stiffness;
                ARef = component.AMatrix.Value.AsReadOnly();
                initialRelativePositions = component.InitialRelativePositions;
                comRef = component.CenterOfMass.Value.AsReadOnly();
            }

            public JobHandle Schedule(JobHandle dependencies)
            {
                return this.Schedule(positions.Length, innerloopBatchCount: 64, dependencies);
            }

            public void Execute(int index)
            {
                var pId = (Id<Point>)index;
                var A = ARef.Value;
                var p = positions[pId];
                var q = initialRelativePositions[pId];
                var com = comRef.Value;

                positions[pId] = math.lerp(p, com + math.mul(A, q), k);
            }
        }

        public override JobHandle Schedule(JobHandle dependencies = default)
        {
            foreach (var component in References)
            {
                dependencies = new CalculateCenterOfMassJob(component).Schedule(dependencies);
                dependencies = new CalculateRelativePositionsJob(component).Schedule(dependencies);
                dependencies = new CalculateApqMatrixJob(component).Schedule(dependencies);
                dependencies = new CalculateRotationMatrixJob(component).Schedule(dependencies);
                dependencies = new ApplyShapeMatchingConstraintJob(component).Schedule(dependencies);
            }

            return dependencies;
        }
    }
}
