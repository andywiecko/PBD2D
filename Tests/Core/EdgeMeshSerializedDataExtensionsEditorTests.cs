using andywiecko.BurstCollections;
using andywiecko.PBD2D.Components;
using andywiecko.PBD2D.Core;
using NUnit.Framework;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace andywiecko.PBD2D.Editor.Tests
{
    public class EdgeMeshSerializedDataExtensionsEditorTests
    {
        private class FakeSerializedData : EdgeMeshSerializedData
        {
            public FakeSerializedData SetPositionsCount(int count) => (Positions = new float2[count], this).Item2;
            public FakeSerializedData SetEdges(int[] edges) => (Edges = edges, this).Item2;
        }

        private static readonly TestCaseData[] extensionToSegmentsTestCaseData =
        {
            new(2, new int[]{ 0, 1 })
            {
                TestName = "Test case 1 (two points)",
                ExpectedResult = new Id<Point>[][]{ new Id<Point>[] { new(0), new(1) } }
            },
            new(4, new int[]{ 0, 1, 1, 2, 2, 3 })
            {
                TestName = "Test case 2 (line)",
                ExpectedResult = new Id<Point>[][]{ new Id<Point>[] { new(0), new(1), new(2), new(3) } }
            },
            new(4, new int[]{ 0, 1, 1, 2, 2, 3, 3, 0 })
            {
                TestName = "Test case 3 (loop)",
                ExpectedResult = new Id<Point>[][]{ new Id<Point>[] { new(0), new(3), new(2), new(1), new(0) } }
            },
            new(5, new int[]{ 0, 1, 0, 2, 0, 3, 0, 4 })
            {
                TestName = "Test case 4 (cross)",
                ExpectedResult = new Id<Point>[][]
                {
                    new Id<Point>[] { new(0), new(4) },
                    new Id<Point>[] { new(0), new(3) },
                    new Id<Point>[] { new(0), new(2) },
                    new Id<Point>[] { new(0), new(1) },
                }
            },
            new(10, new int[]
            {
                0, 1,
                1, 2,
                2, 3,
                2, 4,
                4, 5,
                1, 6,
                6, 7,
                6, 8,
                8, 9,
            })
            {
                TestName = "Test case 5 (tree)",
                ExpectedResult = new Id<Point>[][]
                {
                    new Id<Point>[] { new(1), new(6) },
                    new Id<Point>[] { new(1), new(2) },
                    new Id<Point>[] { new(1), new(0) },
                    new Id<Point>[] { new(2), new(4), new(5) },
                    new Id<Point>[] { new(2), new(3) },
                    new Id<Point>[] { new(6), new(8), new(9) },
                    new Id<Point>[] { new(6), new(7) },
                }
            }
        };

        [Test, TestCaseSource(nameof(extensionToSegmentsTestCaseData))]
        public IEnumerable<IEnumerable<Id<Point>>> ExtensionToSegmentsTest(int count, int[] edges) =>
            ScriptableObject
                .CreateInstance<FakeSerializedData>()
                .SetPositionsCount(count)
                .SetEdges(edges)
                .ToSegments(Allocator.Temp);
    }
}