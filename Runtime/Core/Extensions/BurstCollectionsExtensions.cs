using andywiecko.BurstCollections;

namespace andywiecko.PBD2D.Core
{
    public static class BurstCollectionsExtensions
    {
        public static (T a, T b) At2<T, Edge>(this NativeIndexedArray<Id<Point>, T> array, Edge edge)
            where T : unmanaged
            where Edge : struct, IEdge
        {
            var (idA, idB) = edge;
            return (array[idA], array[idB]);
        }

        public static (T a, T b) At2<T, Edge>(this NativeIndexedArray<Id<Point>, T>.ReadOnly array, Edge edge)
            where T : unmanaged
            where Edge : struct, IEdge
        {
            var (idA, idB) = edge;
            return (array[idA], array[idB]);
        }

        public static (T a, T b, T c) At3<T, Triangle>(this NativeIndexedArray<Id<Point>, T> array, Triangle triangle)
            where T : unmanaged
            where Triangle : struct, ITriangle
        {
            var (idA, idB, idC) = triangle;
            return (array[idA], array[idB], array[idC]);
        }

        public static (T a, T b, T c) At3<T, Triangle>(this NativeIndexedArray<Id<Point>, T>.ReadOnly array, Triangle triangle)
            where T : unmanaged
            where Triangle : struct, ITriangle
        {
            var (idA, idB, idC) = triangle;
            return (array[idA], array[idB], array[idC]);
        }
    }
}