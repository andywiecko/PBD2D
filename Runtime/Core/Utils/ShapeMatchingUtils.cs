using andywiecko.BurstMathUtils;
using andywiecko.ECS;
using System;
using System.Diagnostics;
using Unity.Collections;
using Unity.Mathematics;

namespace andywiecko.PBD2D.Core
{
    [BurstCompatible]
    public static class ShapeMatchingUtils
    {
        public static float2 CalculateCenterOfMass(ReadOnlySpan<float2> positions, ReadOnlySpan<float> massesInv, float totalMass)
        {
            AssertBufferLengths(positions, massesInv);

            var com = (float2)0;
            foreach (var i in 0..positions.Length)
            {
                var p = positions[i];
                var m = 1 / massesInv[i];
                com += m * p;
            }
            com /= totalMass;
            return com;
        }

        public static float CalculateTotalMass(ReadOnlySpan<float> massesInv)
        {
            var M = 0f;
            foreach (var mInv in massesInv)
            {
                M += 1 / mInv;
            }
            return M;
        }

        public static float2x2 CalculateAqqMatrix(ReadOnlySpan<float2> relativePositions, ReadOnlySpan<float> massesInv)
        {
            AssertBufferLengths(relativePositions, massesInv);

            var Aqq = float2x2.zero;
            foreach (var i in 0..relativePositions.Length)
            {
                var q = relativePositions[i];
                var m = 1 / massesInv[i];
                Aqq += m * MathUtils.OuterProduct(q, q);
            }
            return math.inverse(Aqq);
        }

        public static void CalculateRelativePositions(Span<float2> relativePositions, ReadOnlySpan<float2> positions, float2 com)
        {
            AssertBufferLengths(relativePositions, positions);

            foreach (var i in 0..positions.Length)
            {
                relativePositions[i] = positions[i] - com;
            }
        }

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        private static void AssertBufferLengths<T1, T2>(ReadOnlySpan<T1> span0, ReadOnlySpan<T2> span1)
        {
            if (span0.Length != span1.Length)
            {
                throw new ArgumentOutOfRangeException($"Buffers have not the same length! First = {span0.Length}, second = {span1.Length}");
            }
        }

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        private static void AssertBufferLengths<T>(ReadOnlySpan<T> span0, ReadOnlySpan<T> span1)
            => AssertBufferLengths<T, T>(span0, span1);
    }
}