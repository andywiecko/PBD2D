using andywiecko.BurstCollections;
using andywiecko.PBD2D.Core;
using Unity.Mathematics;
using UnityEngine;

namespace andywiecko.PBD2D.Components
{
    [RequireComponent(typeof(TriMesh))]
    [RequireComponent(typeof(TriMeshBoundingVolumeTreePoints))]
    [AddComponentMenu("PBD2D:TriMesh.Components/Collisions/Collide With Ground (Points)")]
    public class TriMeshPointsCollideWithGroundLine : BaseComponent, ITriMeshPointsCollideWithGroundLine
    {
        public AABB Bounds => treeComponent.Bounds;
        public float CollisionRadius => 0;
        public Ref<NativeIndexedArray<Id<Point>, float2>> PredictedPositions => triMesh.PredictedPositions;
        public Ref<NativeIndexedArray<Id<Point>, float2>> Positions => triMesh.Positions;
        public float Friction => triMesh.PhysicalMaterial.Friction;

        private TriMesh triMesh;
        private TriMeshBoundingVolumeTreePoints treeComponent;

        private void Start()
        {
            triMesh = GetComponent<TriMesh>();
            treeComponent = GetComponent<TriMeshBoundingVolumeTreePoints>();
        }
    }
}