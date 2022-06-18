using UnityEngine;

namespace andywiecko.PBD2D.Core
{
    public class RangeMinMaxAttribute : PropertyAttribute
    {
        public float Min { get; }
        public float Max { get; }
        public RangeMinMaxAttribute(float min, float max) => (Min, Max) = (min, max);
        public void Deconstruct(out float min, out float max) => (min, max) = (Min, Max);
    }
}