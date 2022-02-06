using andywiecko.BurstCollections;
using andywiecko.BurstMathUtils;
using andywiecko.PBD2D.Core;
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

            DisposeOnDestroy(
                initialRelativePositions = new NativeIndexedArray<Id<Point>, float2>(pointsCount, Allocator.Persistent),
                RelativePositions = new NativeIndexedArray<Id<Point>, float2>(pointsCount, Allocator.Persistent),
                CenterOfMass = new NativeReference<float2>(Allocator.Persistent),
                ApqMatrix = new NativeReference<float2x2>(Allocator.Persistent),
                AMatrix = new NativeReference<float2x2>(Allocator.Persistent),
                Rotation = new NativeReference<Complex>(Complex.Identity, Allocator.Persistent)
            );

            TotalMass = ShapeMatchingUtils.CalculateTotalMass(MassesInv);
            CenterOfMass.Value.Value = ShapeMatchingUtils.CalculateCenterOfMass(triMesh.Positions.Value, MassesInv, TotalMass);
            ShapeMatchingUtils.CalculateRelativePositions(initialRelativePositions, triMesh.Positions.Value, CenterOfMass.Value.Value);
            AqqMatrix = ShapeMatchingUtils.CalculateAqqMatrix(initialRelativePositions, MassesInv);
        }

        private void OnDrawGizmos()
        {
            if(!Application.isPlaying)
            {
                return;
            }

            DrawRotation();
        }

        private void DrawRotation()
        {
            var com = CenterOfMass.Value.Value.ToFloat3();
            var R = Rotation.Value.Value;
            var right = R.Value.ToFloat3();
            var up = MathUtils.Rotate90CCW(R.Value).ToFloat3();

            Gizmos.color = Color.red;
            Gizmos.DrawRay(com, right);
            Gizmos.color = Color.green;
            Gizmos.DrawRay(com, up);
        }
    }
}
