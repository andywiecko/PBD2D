using andywiecko.BurstCollections;

namespace andywiecko.PBD2D.Core
{
    public static class BurstCollectionsExtensions
    {
        public static T At<T>(this NativeIndexedArray<Id<Point>, T> array, Point point) where T : unmanaged => array[point.Id];
        public static T At<T>(this NativeIndexedArray<Id<Point>, T>.ReadOnly array, Point point) where T : unmanaged => array[point.Id];
        public static (T a, T b) At<T>(this NativeIndexedArray<Id<Point>, T> array, Edge edge) where T : unmanaged => (array[edge.IdA], array[edge.IdB]);
        public static (T a, T b) At<T>(this NativeIndexedArray<Id<Point>, T>.ReadOnly array, Edge edge) where T : unmanaged => (array[edge.IdA], array[edge.IdB]);
        public static (T a, T b, T c) At<T>(this NativeIndexedArray<Id<Point>, T> array, Triangle triangle) where T : unmanaged =>
            (array[triangle.IdA], array[triangle.IdB], array[triangle.IdC]);
        public static (T a, T b, T c) At<T>(this NativeIndexedArray<Id<Point>, T>.ReadOnly array, Triangle triangle) where T : unmanaged =>
            (array[triangle.IdA], array[triangle.IdB], array[triangle.IdC]);
    }
}