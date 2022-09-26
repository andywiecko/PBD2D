using Unity.Mathematics;
using UnityEngine;

namespace andywiecko.PBD2D.Components
{
    public abstract class RodSerializedData : ScriptableObject
    {
        [field: SerializeField, HideInInspector] public float2[] Positions { get; protected set; } = { };
        [field: SerializeField, HideInInspector] public int[] Edges { get; protected set; } = { };
    }
}
