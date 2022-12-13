using Unity.Mathematics;
using UnityEngine;

namespace andywiecko.PBD2D.Components
{
    public abstract class EdgeMeshSerializedData : ScriptableObject
    {
        [field: SerializeField, HideInInspector] public virtual float2[] Positions { get; protected set; } = { };
        [field: SerializeField, HideInInspector] public virtual int[] Edges { get; protected set; } = { };
    }
}
