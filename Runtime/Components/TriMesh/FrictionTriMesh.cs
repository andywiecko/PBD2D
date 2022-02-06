using andywiecko.BurstCollections;
using andywiecko.PBD2D.Core;
using Unity.Mathematics;
using UnityEngine;

namespace andywiecko.PBD2D.Components
{
    [RequireComponent(typeof(TriMesh))]
    [AddComponentMenu("PBD2D:TriMesh.Components/Collisions/Friction")]
    public class FrictionTriMesh : BaseComponent, IFrictionComponent
    {
        public Ref<NativeIndexedArray<Id<Point>, Friction>> AccumulatedFriction => triMesh.AccumulatedFriction;
        public Ref<NativeIndexedArray<Id<Point>, float2>> PredictedPositions => triMesh.PredictedPositions;
        public NativeIndexedArray<Id<Point>, float2>.ReadOnly Positions => triMesh.Positions.Value.AsReadOnly();

        private TriMesh triMesh;

        private void Awake()
        {
            triMesh = GetComponent<TriMesh>();
        }
    }
}