using andywiecko.BurstCollections;
using andywiecko.PBD2D.Core;
using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace andywiecko.PBD2D.Systems
{
    [AddComponentMenu("PBD2D:Systems/Constraints/Edge Length Constraint System")]
    public class EdgeLengthConstraintSystem : BaseSystem<IEdgeLengthConstraint>
    {
        [BurstCompile]
        private struct ApplyEdgeConstraintJob : IJob
        {
            private NativeIndexedArray<Id<Point>, float2> predictedPositions;
            private NativeIndexedArray<Id<Edge>, Edge>.ReadOnly edges;
            private NativeIndexedArray<Id<Point>, float>.ReadOnly massesInv;
            private NativeIndexedArray<Id<Edge>, float>.ReadOnly restLengths;
            private readonly float stiffness;

            public ApplyEdgeConstraintJob(IEdgeLengthConstraint constraint)
            {
                predictedPositions = constraint.PredictedPositions;
                edges = constraint.Edges;
                massesInv = constraint.MassesInv;
                restLengths = constraint.RestLengths;
                stiffness = constraint.Stiffness;
            }

            public void Execute()
            {
                foreach (var i in 0..edges.Length)
                {
                    Execute((Id<Edge>)i);
                }
            }

            private void Execute(Id<Edge> edgeId)
            {
                var restLenght = restLengths[edgeId];
                var edge = edges[edgeId];
                var (idA, idB) = edge;

                var (wA, wB) = massesInv.At(edge);
                var wAB = wA + wB;
                if (wAB <= Constants.FloatEpsilon)
                {
                    return;
                }

                var (pA, pB) = predictedPositions.At(edge);
                var pAB = pB - pA;
                var length = math.length(pAB);
                if (length < Constants.FloatEpsilon)
                {
                    return;
                }

                var dP = stiffness * (1f / wAB) * (length - restLenght) * pAB / length;

                predictedPositions[idA] += wA * dP;
                predictedPositions[idB] -= wB * dP;
            }
        }

        public override JobHandle Schedule(JobHandle dependencies = default)
        {
            foreach (var constraint in References)
            {
                if (constraint.Stiffness != 0)
                {
                    dependencies = new ApplyEdgeConstraintJob(constraint).Schedule(dependencies);
                }
            }

            return dependencies;
        }
    }
}
