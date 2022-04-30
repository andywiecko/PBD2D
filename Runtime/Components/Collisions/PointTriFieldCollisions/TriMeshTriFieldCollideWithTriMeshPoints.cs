using andywiecko.BurstCollections;
using andywiecko.PBD2D.Core;
using Unity.Mathematics;
using UnityEngine;

namespace andywiecko.PBD2D.Components
{
    [RequireComponent(typeof(TriMesh))]
    [RequireComponent(typeof(TriMeshExternalEdges))]
    [RequireComponent(typeof(TriMeshTriField))]
    [RequireComponent(typeof(TriMeshBoundingVolumeTreeTriangles))]
    [AddComponentMenu("PBD2D:TriMesh.Components/Collisions/Collide With TriMesh (TriField with Points)")]
    public class TriMeshTriFieldCollideWithTriMeshPoints : BaseComponent, ITriMeshTriFieldCollideWithTriMeshPoints
    {
        public Ref<NativeIndexedArray<Id<Point>, float2>> PredictedPositions => triMesh.PredictedPositions;
        public Ref<NativeIndexedArray<Id<Point>, float>> MassesInv => triMesh.MassesInv;
        public Ref<NativeIndexedArray<Id<Triangle>, Triangle>> Triangles => triMesh.Triangles;
        public Ref<NativeIndexedArray<Id<ExternalEdge>, ExternalEdge>> ExternalEdges => triMeshExternalEdges.ExternalEdges;
        public Ref<TriFieldLookup> TriFieldLookup => triFieldComponent.TriFieldLookup;
        public Ref<NativeBoundingVolumeTree<AABB>> Tree => treeComponent.Tree;

        private TriMesh triMesh;
        private TriMeshExternalEdges triMeshExternalEdges;
        private TriMeshBoundingVolumeTreeTriangles treeComponent;
        private TriMeshTriField triFieldComponent;

        private void Start()
        {
            triMesh = GetComponent<TriMesh>();
            triMeshExternalEdges = GetComponent<TriMeshExternalEdges>();
            triFieldComponent = GetComponent<TriMeshTriField>();
            treeComponent = GetComponent<TriMeshBoundingVolumeTreeTriangles>();
        }
    }
}
