using andywiecko.BurstCollections;

namespace andywiecko.PBD2D.Core
{
    public static class BurstCollectionsExtensions
    {
        public static (T a, T b, T c) At<T>(this NativeIndexedArray<Id<Point>, T> array, Triangle triangle) where T : unmanaged
        {
            var (idA, idB, idC) = triangle;
            return (array[idA], array[idB], array[idC]);
        }

        public static (T a, T b, T c) At<T>(this NativeIndexedArray<Id<Point>, T>.ReadOnly array, Triangle triangle) where T : unmanaged
        {
            var (idA, idB, idC) = triangle;
            return (array[idA], array[idB], array[idC]);
        }

        public static (T a, T b) At<T>(this NativeIndexedArray<Id<Point>, T> array, Edge edge) where T : unmanaged
        {
            var (idA, idB) = edge;
            return (array[idA], array[idB]);
        }

        public static (T a, T b) At<T>(this NativeIndexedArray<Id<Point>, T>.ReadOnly array, Edge edge) where T : unmanaged
        {
            var (idA, idB) = edge;
            return (array[idA], array[idB]);
        }
    }
}