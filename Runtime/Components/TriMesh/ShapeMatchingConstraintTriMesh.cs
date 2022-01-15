using andywiecko.BurstCollections;
using andywiecko.BurstMathUtils;
using andywiecko.PBD2D.Core;
using System.Linq;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace andywiecko.PBD2D.Components
{
    [RequireComponent(typeof(TriMesh))]
    [AddComponentMenu("PBD2D:TriMesh.Components/Constraints/Shape Matching Constraint")]
    public class ShapeMatchingConstraintTriMesh : BaseComponent, IShapeMatchingConstraint
    {
        [field: SerializeField, Range(0, 1)]
        public float Stiffness { get; private set; } = 1;

        public float TotalMass { get; private set; }

        public NativeIndexedArray<Id<Point>, float>.ReadOnly MassesInv => triMesh.MassesInv.Value.AsReadOnly();
        public Ref<NativeIndexedArray<Id<Point>, float2>> PredictedPositions => triMesh.PredictedPositions;

        public NativeIndexedArray<Id<Point>, float2>.ReadOnly InitialRelativePositions => initialRelativePositions.AsReadOnly();
        private NativeIndexedArray<Id<Point>, float2> initialRelativePositions;

        public Ref<NativeIndexedArray<Id<Point>, float2>> RelativePositions { get; private set; }
        public Ref<NativeReference<float2>> CenterOfMass { get; private set; }
        public Ref<NativeReference<float2x2>> ApqMatrix { get; private set; }
        public float2x2 AqqMatrix { get; private set; }
        public Ref<NativeReference<float2x2>> AMatrix { get; private set; }
        public Ref<NativeReference<Complex>> Rotation { get; private set; }

        [field: SerializeField, Range(0, 1)]
        public float Beta { get; private set; } = 0;

        private TriMesh triMesh;

        private void Start()
        {
            triMesh = GetComponent<TriMesh>();
            var pointsCount = triMesh.Positions.Value.Length;

            var M = 0f;
            var com = float2.zero;
            foreach (var (pId, p) in triMesh.Positions.Value.IdsValues)
            {
                var m = 1 / MassesInv[pId];
                com += m * p;
                M += m;
            }
            com /= M;
            TotalMass = M;

            var managedInitialRelativePositions = triMesh.Positions.Value.Select(i => i - com).ToArray();

            DisposeOnDestroy(
                initialRelativePositions = new NativeIndexedArray<Id<Point>, float2>(managedInitialRelativePositions, Allocator.Persistent),
                RelativePositions = new NativeIndexedArray<Id<Point>, float2>(pointsCount, Allocator.Persistent),
                CenterOfMass = new NativeReference<float2>(Allocator.Persistent),
                ApqMatrix = new NativeReference<float2x2>(Allocator.Persistent),
                AMatrix = new NativeReference<float2x2>(Allocator.Persistent),
                Rotation = new NativeReference<Complex>(Complex.Identity, Allocator.Persistent)
            );

            var Aqq = float2x2.zero;
            foreach (var (pId, p) in triMesh.Positions.Value.IdsValues)
            {
                var m = 1 / triMesh.MassesInv.Value[pId];
                var q = p - com;
                Aqq += m * MathUtils.OuterProduct(q, q);
            }
            AqqMatrix = math.inverse(Aqq);
        }
    }
}
