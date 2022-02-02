using Unity.Mathematics;
using UnityEngine;

namespace andywiecko.PBD2D.Components
{
    public abstract class TriMeshSerializedData : ScriptableObject
    {
        public abstract Mesh Mesh { get; protected set; }
        public abstract float[] MassesInv { get; protected set; }
        public abstract float2[] Positions { get; protected set; }
        public abstract int[] Edges { get; protected set; }
        public abstract float[] RestLengths { get; protected set; }
        public abstract int[] Triangles { get; protected set; }
        public abstract float[] RestAreas2 { get; protected set; }
        public abstract Vector2[] UVs { get; protected set; }
    }
}