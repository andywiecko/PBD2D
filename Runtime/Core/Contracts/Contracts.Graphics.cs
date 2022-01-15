using andywiecko.BurstCollections;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace andywiecko.PBD2D.Core
{
    public interface IHermiteSplineRendererRod : IComponent
    {
        NativeIndexedArray<Id<Edge>, Edge>.ReadOnly Edges { get; }
        NativeIndexedArray<Id<Point>, float2>.ReadOnly Positions { get; }
        Ref<NativeIndexedArray<Id<Edge>, FixedList128Bytes<float2>>> TmpPointsToRender { get; }
        Ref<NativeList<float3>> PointsToRender { get; }
        int PointsPerCurveSegment { get; }
        LineRenderer LineRenderer { get; }
    }

    public interface ITriMeshRenderer : IComponent
    {
        NativeIndexedArray<Id<Point>, float2>.ReadOnly Positions { get; }
        Ref<NativeArray<float3>> MeshVertices { get; }
        void Redraw();
    }
}