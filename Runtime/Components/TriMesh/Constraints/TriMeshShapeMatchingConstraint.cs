using andywiecko.BurstCollections;
using andywiecko.BurstMathUtils;
using andywiecko.ECS;
using andywiecko.PBD2D.Core;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace andywiecko.PBD2D.Components
{
    [RequireComponent(typeof(TriMesh))]
    [Category(PBDCategory.Constraints)]
    public class TriMeshShapeMatchingConstraint : BaseComponent, IShapeMatchingConstraint
    {
        [field: SerializeField, Range(0, 1)]
        public float Stiffness { get; private set; } = 1;

        public float TotalMass { get; private set; }

        public Ref<NativeIndexedArray<Id<Point>, float>> Weights => triMesh.Weights;
        public Ref<NativeIndexedArray<Id<Point>, float2>> Positions => triMesh.Positions;
        public Ref<NativeIndexedArray<Id<Point>, float2>> InitialRelativePositions { get; private set; }
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
                InitialRelativePositions = new NativeIndexedArray<Id<Point>, float2>(pointsCount, Allocator.Persistent),
                RelativePositions = new NativeIndexedArray<Id<Point>, float2>(pointsCount, Allocator.Persistent),
                CenterOfMass = new NativeReference<float2>(Allocator.Persistent),
                ApqMatrix = new NativeReference<float2x2>(Allocator.Persistent),
                AMatrix = new NativeReference<float2x2>(Allocator.Persistent),
                Rotation = new NativeReference<Complex>(Complex.Identity, Allocator.Persistent)
            );

            TotalMass = ShapeMatchingUtils.CalculateTotalMass(Weights.Value);
            CenterOfMass.Value.Value = ShapeMatchingUtils.CalculateCenterOfMass(triMesh.Positions.Value, Weights.Value, TotalMass);
            ShapeMatchingUtils.CalculateRelativePositions(InitialRelativePositions.Value, triMesh.Positions.Value, CenterOfMass.Value.Value);
            AqqMatrix = ShapeMatchingUtils.CalculateAqqMatrix(InitialRelativePositions.Value, Weights.Value);
        }

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            DrawRotation();
        }

        private void DrawRotation()
        {
            var com = CenterOfMass.Value.Value;
            var R = Rotation.Value.Value;
            var right = R.Value;
            var up = MathUtils.Rotate90CCW(R.Value);

            Gizmos.color = Color.red;
            GizmosUtils.DrawRay(com, right);
            Gizmos.color = Color.green;
            GizmosUtils.DrawRay(com, up);
        }
    }
}
