using Unity.Mathematics;
using UnityEngine;

namespace andywiecko.PBD2D.Components
{
    public abstract class TriMeshSerializedData : ScriptableObject
    {
        [field: SerializeField, HideInInspector] public Mesh Mesh { get; protected set; } = default;
        [field: SerializeField, HideInInspector] public float2[] Positions { get; protected set; } = { };
        [field: SerializeField, HideInInspector] public int[] Triangles { get; protected set; } = { };
        [field: SerializeField, HideInInspector] public Vector2[] UVs { get; protected set; } = { };
    }
}