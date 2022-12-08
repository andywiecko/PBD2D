using andywiecko.BurstCollections;
using andywiecko.ECS;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace andywiecko.PBD2D.Core
{
    public interface ITriMeshRenderer : IComponent
    {
        float4x4 WorldToLocal { get; }
        Mesh Mesh { get; }
        Ref<NativeIndexedArray<Id<Point>, float2>> Positions { get; }
        Ref<NativeArray<float3>> MeshVertices { get; }
    }

    public interface IEdgeMeshRenderer : IComponent
    {
        IReadOnlyList<LineRenderer> Renderers { get; }
        Ref<NativeStackedLists<Id<Point>>> Segments { get; }
        Ref<NativeIndexedArray<Id<Point>, float2>> Positions { get; }
        Ref<NativeStackedLists<float3>> MeshVertices { get; }
    }
}