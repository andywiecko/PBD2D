using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

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

        public static RangeEnumerator GetEnumerator(this Range range) => new(range);

        public static string ToNonPascal(this string s) => Regex.Replace(s, "[A-Z]", " $0").Trim();

        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            var seenKeys = new HashSet<TKey>();
            foreach (TSource element in source)
            {
                if (seenKeys.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
        }
    }
}