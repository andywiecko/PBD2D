using andywiecko.PBD2D.Core;
using System.Linq;

namespace andywiecko.PBD2D.Components
{
    public static class TriMeshSerializedDataExtensions
    {
        public static Triangle[] ToTrianglesArray(this TriMeshSerializedData data) => Enumerable
            .Range(0, data.Triangles.Length / 3)
            .Select(i => (Triangle)(data.Triangles[3 * i], data.Triangles[3 * i + 1], data.Triangles[3 * i + 2]))
            .ToArray();
    }
}