using andywiecko.BurstMathUtils;
using System;
using Unity.Collections;
using Unity.Mathematics;

namespace andywiecko.PBD2D.Core
{
    [BurstCompatible]
    public static class ShapeMatchingUtils
    {
        public static float CalculateTotalMass(ReadOnlySpan<Point> points, ReadOnlySpan<float> weights)
        {
            var M = 0f;
            foreach (var p in points)
            {
                M += 1 / weights[(int)p.Id];
            }
            return M;
        }

        public static float2x2 CalculateAqqMatrix(ReadOnlySpan<PointShapeMatchingConstraint> constraints, ReadOnlySpan<float> weights)
        {
            var Aqq = float2x2.zero;
            foreach (var (i, _, q) in constraints)
            {
                var m = 1 / weights[(int)i];
                Aqq += m * MathUtils.OuterProduct(q, q);
            }
            return math.inverse(Aqq);
        }

        public static float2x2 CalculateApqMatrix(ReadOnlySpan<PointShapeMatchingConstraint> constraints, ReadOnlySpan<float> weights)
        {
            var Apq = float2x2.zero;
            foreach (var (i, p, q) in constraints)
            {
                var m = 1 / weights[(int)i];
                Apq += m * MathUtils.OuterProduct(p, q);
            }
            return Apq;
        }

        public static void GenerateConstraints(NativeList<PointShapeMatchingConstraint> constraints, ReadOnlySpan<Point> points, ReadOnlySpan<float2> positions, float2 com)
        {
            foreach (var p in points)
            {
                var id = p.Id;
                var q = positions[(int)id] - com;
                constraints.Add(new(id, q));
            }
        }

        public static float2 CalculateCenterOfMass<T>(ReadOnlySpan<T> points, ReadOnlySpan<float2> positions, ReadOnlySpan<float> weights, float totalMass)
            where T : struct, IPoint
        {
            var com = float2.zero;
            foreach (var p in points)
            {
                var id = (int)p.Id;
                var q = positions[id];
                var m = 1 / weights[id];
                com += m * q;
            }
            return com / totalMass;
        }
    }
}