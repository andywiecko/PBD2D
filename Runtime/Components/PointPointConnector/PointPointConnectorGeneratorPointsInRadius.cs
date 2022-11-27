using andywiecko.BurstCollections;
using andywiecko.ECS;
using andywiecko.PBD2D.Core;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace andywiecko.PBD2D.Components
{
    [Category(PBDCategory.Generator)]
    [RequireComponent(typeof(PointPointConnector))]
    public class PointPointConnectorGeneratorPointsInRadius : PointPointConnectorGenerator
    {
        [SerializeField]
        private float radius = 0.01f;

        public override void GenerateConstraints() => new GenerateConstraintsJob(connector, radius).Run();

        [BurstCompile]
        private struct GenerateConstraintsJob : IJob
        {
            private NativeList<PointPointConnectorConstraint> constraints;
            private NativeArray<Point>.ReadOnly pointsA;
            [ReadOnly]
            private NativeArray<Point> pointsB;
            private NativeIndexedArray<Id<Point>, float2>.ReadOnly positionsA;
            private NativeIndexedArray<Id<Point>, float2>.ReadOnly positionsB;
            private readonly float radiusSq;

            public GenerateConstraintsJob(PointPointConnector connector, float radius)
            {
                constraints = connector.Constraints;
                radiusSq = radius * radius;

                var connectee = connector.Connectee;
                var connecter = connector.Connecter;
                pointsA = connectee.Points.Value.AsReadOnly();
                pointsB = connecter.Points.Value;
                positionsA = connectee.Positions.Value.AsReadOnly();
                positionsB = connecter.Positions.Value.AsReadOnly();
            }

            public void Execute()
            {
                using var pointsB = new NativeList<Point>(Allocator.Temp);
                pointsB.CopyFrom(this.pointsB);
                constraints.Capacity = math.min(pointsA.Length, pointsB.Length);

                foreach (var a in pointsA)
                {
                    var pA = positionsA[a.Id];
                    for (int i = pointsB.Length - 1; i >= 0; i--)
                    {
                        var b = pointsB[i];
                        var pB = positionsB[b.Id];
                        if (math.distancesq(pA, pB) <= radiusSq)
                        {
                            pointsB.RemoveAtSwapBack(i);
                            constraints.AddNoResize(new(a.Id, b.Id));
                        }
                    }
                }
            }
        }
    }
}