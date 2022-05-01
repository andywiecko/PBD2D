using andywiecko.BurstCollections;
using andywiecko.PBD2D.Core;
using Unity.Mathematics;
using UnityEngine;

namespace andywiecko.PBD2D.Components
{
    [RequireComponent(typeof(TriMesh))]
    [RequireComponent(typeof(TriMeshBoundingVolumeTreePoints))]
    [AddComponentMenu("PBD2D:TriMesh.Components/Collisions/Collide With TriMesh (Points with TriField)")]
    public class TriMeshPointsCollideWithTriMeshTriField : BaseComponent, ITriMeshPointsCollideWithTriMeshTriField
    {
        public Ref<NativeIndexedArray<Id<Point>, float2>> PredictedPositions => triMesh.PredictedPositions;
        public Ref<NativeIndexedArray<Id<Point>, float>> MassesInv => triMesh.MassesInv;
        public Ref<NativeBoundingVolumeTree<AABB>> Tree => treeComponent.Tree;
        public AABB Bounds => treeComponent.Bounds;

        private TriMesh triMesh;
        private TriMeshBoundingVolumeTreePoints treeComponent;

        private void Start()
        {
            triMesh = GetComponent<TriMesh>();
            treeComponent = GetComponent<TriMeshBoundingVolumeTreePoints>();
        }
    }
}
