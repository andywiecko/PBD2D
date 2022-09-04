using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine.TestTools.Utils;

namespace andywiecko.PBD2D.Editor.Tests
{
    public class Float2Comparer : IEqualityComparer<float2>
    {
        private const float DefaultEpsilon = 0.0001f;
        private readonly float epsilon;

        public static readonly Float2Comparer Instance = new(DefaultEpsilon);
        public Float2Comparer(float epsilon) => this.epsilon = epsilon;

        public bool Equals(float2 expected, float2 actual) => true
            && Utils.AreFloatsEqual(expected.x, actual.x, epsilon)
            && Utils.AreFloatsEqual(expected.y, actual.y, epsilon)
        ;

        public int GetHashCode(float2 _) => 0;
    }
}
