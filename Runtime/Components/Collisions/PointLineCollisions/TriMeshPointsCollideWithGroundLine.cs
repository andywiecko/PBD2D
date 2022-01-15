using andywiecko.BurstCollections;
using andywiecko.PBD2D.Core;
using Unity.Mathematics;
using UnityEngine;

namespace andywiecko.PBD2D.Components
{
    [RequireComponent(typeof(TriMesh))]
    [AddComponentMenu("PBD2D:TriMesh.Components/Collisions/Collide With Ground (Points)")]
    public class TriMeshPointsCollideWithGroundLine : BaseComponent, ITriMeshPointsCollideWithGroundLine
    {
        public float CollisionRadius => 0;
        public Ref<NativeIndexedArray<Id<Point>, float2>> PredictedPositions => triMesh.PredictedPositions;

        private TriMesh triMesh;

        private void Start()
        {
            triMesh = GetComponent<TriMesh>();
        }
    }
}