using andywiecko.BurstCollections;
using andywiecko.ECS;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace andywiecko.PBD2D.Core
{
    public interface ITriMeshRenderer : IComponent
    {
        Mesh Mesh { get; }
        Ref<NativeIndexedArray<Id<Point>, float2>> Positions { get; }
        Ref<NativeArray<float3>> MeshVertices { get; }
    }
}