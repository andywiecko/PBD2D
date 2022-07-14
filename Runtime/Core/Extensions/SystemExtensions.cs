using System;

namespace andywiecko.PBD2D.Core
{
    public static class SystemExtensions
    {
        public struct RangeEnumerator
        {
            private readonly Range range;
            public RangeEnumerator(Range range)
            {
                this.range = range;
                Current = range.Start.Value - 1;
            }
            public int Current { get; private set; }
            public bool MoveNext() => ++Current < range.End.Value;
            public RangeEnumerator GetEnumerator() => this;
        }

        public static T[] GetValues<T>() where T : Enum => (T[])Enum.GetValues(typeof(T));
        public static RangeEnumerator GetEnumerator(this Range range) => new(range);
    }
}