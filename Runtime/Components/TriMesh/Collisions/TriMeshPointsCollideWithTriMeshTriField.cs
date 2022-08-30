using andywiecko.BurstCollections;
using andywiecko.ECS;
using andywiecko.PBD2D.Core;
using Unity.Mathematics;
using UnityEngine;

namespace andywiecko.PBD2D.Components
{
    [RequireComponent(typeof(TriMesh))]
    [RequireComponent(typeof(TriMeshBoundingVolumeTreePoints))]
    [Category(PBDCategory.Collisions)]
    public class TriMeshPointsCollideWithTriMeshTriField : BaseComponent, ITriMeshPointsCollideWithTriMeshTriField
    {
        public Ref<NativeIndexedArray<Id<Point>, float2>> Positions => triMesh.Positions;
        public Ref<NativeIndexedArray<Id<Point>, float2>> PreviousPositions => triMesh.PreviousPositions;
        public Ref<NativeIndexedArray<Id<Point>, float>> Weights => triMesh.Weights;
        public Ref<NativeBoundingVolumeTree<AABB>> Tree => treeComponent.Tree;
        public AABB Bounds => treeComponent.Bounds;
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
