using andywiecko.BurstCollections;

namespace andywiecko.PBD2D.Core
{
    public static class NativeIndexedArrayPointExtensions
    {
        public static T At<T, U>(this NativeIndexedArray<Id<Point>, T> array, U point)
            where T : unmanaged where U : unmanaged, IPoint => array[point.Id];
        public static T At<T, U>(this NativeIndexedArray<Id<Point>, T>.ReadOnly array, U point)
            where T : unmanaged where U : unmanaged, IPoint => array[point.Id];
    }

    public static class NativeIndexedArrayEdgeExtensions
    {
        public static (T a, T b) At<T, U>(this NativeIndexedArray<Id<Point>, T> array, U edge)
            where T : unmanaged where U : unmanaged, IEdge => (array[edge.IdA], array[edge.IdB]);
        public static (T a, T b) At<T, U>(this NativeIndexedArray<Id<Point>, T>.ReadOnly array, U edge)
            where T : unmanaged where U : unmanaged, IEdge => (array[edge.IdA], array[edge.IdB]);
    }

    public static class NativeIndexedArrayTriangleExtensions
    {
        public static (T a, T b, T c) At<T, U>(this NativeIndexedArray<Id<Point>, T> array, U triangle)
            where T : unmanaged where U : unmanaged, ITriangle => (array[triangle.IdA], array[triangle.IdB], array[triangle.IdC]);
        public static (T a, T b, T c) At<T, U>(this NativeIndexedArray<Id<Point>, T>.ReadOnly array, U triangle)
            where T : unmanaged where U : unmanaged, ITriangle => (array[triangle.IdA], array[triangle.IdB], array[triangle.IdC]);
    }
}

