using andywiecko.BurstCollections;
using andywiecko.BurstMathUtils;
using andywiecko.ECS;
using andywiecko.PBD2D.Core;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace andywiecko.PBD2D.Components
{
    [Category(PBDCategory.Constraints)]
    [RequireComponent(typeof(Rod))]
    public class RodBendingAngleConstraints : BaseComponent, IBendingAngleConstraints
    {
        [field: SerializeField, Range(0, 1)]
        public float Stiffness { get; private set; } = 1;

        [field: SerializeField, Min(0)]
        public float Compliance { get; private set; } = 0;

        public Ref<NativeIndexedArray<Id<Point>, float2>> Positions => rod.Positions;
        public Ref<NativeIndexedArray<Id<Point>, float>> Weights => rod.Weights;
        public Ref<NativeList<BendingAngleConstraint>> Constraints { get; private set; }

        private Rod rod;

        private void Start()
        {
            rod = GetComponent<Rod>();

            var count = rod.EdgeNeighbors.Value.Count();
            DisposeOnDestroy(
                Constraints = new NativeList<BendingAngleConstraint>(count, Allocator.Persistent)
            );

            using var visited = new NativeList<IdPair<Edge>>(count, Allocator.Temp);
            var edges = rod.Edges.Value.AsReadOnly();
            var positions = rod.Positions.Value.AsReadOnly();
            foreach (var i in rod.EdgeNeighbors.Value)
            {
                var (eA, eB) = (i.Key, i.Value);
                (eA, eB) = eA < eB ? (eA, eB) : (eB, eA);
                var pair = new IdPair<Edge>(eA, eB);
                if (!visited.Contains(pair))
                {
                    visited.Add(pair);
                    var ((a, b), (c, d)) = (edges[eA], edges[eB]);
                    var (x, y, z) = (b == c, a == c, b == d, a == d) switch
                    {
                        (true, _, _, _) => (a, b, d),
                        (_, true, _, _) => (b, a, d),
                        (_, _, true, _) => (a, b, c),
                        (_, _, _, true) => (b, a, c),
                        _ => throw new System.Exception()
                    };

                    var (pA, pB, pC) = positions.At((Triangle)(x, y, z));
                    var (t1, t2) = (pB - pA, pC - pB);
                    var cross = MathUtils.Cross(t1, t2);
                    var dot = math.dot(t1, t2);
                    var angle = math.atan2(cross, dot);

                    Constraints.Value.AddNoResize(new(x, y, z, angle));
                }
            }
        }
    }
}