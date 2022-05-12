using andywiecko.BurstCollections;
using andywiecko.PBD2D.Core;
using NUnit.Framework;
using Unity.Collections;
using Unity.Mathematics;

namespace andywiecko.PBD2D.Editor.Tests
{
    public class TriFieldLookupEditorTests
    {
        [Test]
        public void SingleTriangleManySamplesTest()
        {
            using var positions = new NativeIndexedArray<Id<Point>, float2>(
                new float2[] { new(0, 0), new(1, 0), new(1, 1) }, Allocator.Persistent);
            using var triangles = new NativeIndexedArray<Id<Triangle>, Triangle>(
                new Triangle[] { (0, 1, 2) }, Allocator.Persistent);
            using var externalEdges = new NativeIndexedArray<Id<ExternalEdge>, ExternalEdge>(
                new ExternalEdge[] { (0, 1), (1, 2), (2, 0) }, Allocator.Persistent);

            using var field = new TriFieldLookup(trianglesCount: 1, samples: 3, Allocator.Persistent);
            field.Initialize(positions.AsReadOnly(), triangles.AsReadOnly(), externalEdges.AsReadOnly(), default).Complete();

            var a = (int)field.GetExternalEdge(default, bar: new(.5f, .5f, 0));
            var b = (int)field.GetExternalEdge(default, bar: new(0, .5f, .5f));
            var c = (int)field.GetExternalEdge(default, bar: new(.5f, 0, .5f));

            Assert.That((a, b, c), Is.EqualTo((0, 1, 2)));
        }

        [Test]
        public void ManyTrianglesSingleSampleTest()
        {
            using var positions = new NativeIndexedArray<Id<Point>, float2>(
                new float2[]
                {
                    new(0, 0), new(1, 0), new(2, 0),
                    new(0, 1), new(1, 1), new(2, 1),
                    new(0, 2), new(1, 2), new(2, 2),
                }, Allocator.Persistent);
            using var triangles = new NativeIndexedArray<Id<Triangle>, Triangle>(
                new Triangle[]
                {
                    (0, 1, 4), (0, 3, 4), (1, 4, 5), (1, 2, 5),
                    (3, 4, 7), (3, 6, 7), (4, 7, 8), (4, 5, 8),
                }, Allocator.Persistent);
            using var externalEdges = new NativeIndexedArray<Id<ExternalEdge>, ExternalEdge>(
                new ExternalEdge[]
                {
                    (0, 1), (1, 2), (2, 5), (5, 8),
                    (8, 7), (7, 6), (6, 3), (3, 0),
                }, Allocator.Persistent);

            using var field = new TriFieldLookup(trianglesCount: 8, samples: 1, Allocator.Persistent);
            field.Initialize(positions.AsReadOnly(), triangles.AsReadOnly(), externalEdges.AsReadOnly(), default).Complete();

            var a = (int)field.GetExternalEdge((Id<Triangle>)0, default);
            var b = (int)field.GetExternalEdge((Id<Triangle>)1, default);
            var c = (int)field.GetExternalEdge((Id<Triangle>)3, default);

            Assert.That((a, b, c), Is.EqualTo((0, 7, 2)));
        }
    }
}