using andywiecko.BurstCollections;
using andywiecko.PBD2D.Core;
using Unity.Mathematics;
using UnityEngine;

namespace andywiecko.PBD2D.Components
{
    [RequireComponent(typeof(TriMesh))]
    [AddComponentMenu("PBD2D:TriMesh.Components/Collisions/Collide With Ground")]
    public class TriMeshCollideWithGround : BaseComponent, ITriMeshCollideWithGround
    {
        public Ref<NativeIndexedArray<Id<Point>, float2>> PredictedPositions => triMesh.PredictedPositions;

        private TriMesh triMesh;

        private void Start()
        {
            triMesh = GetComponent<TriMesh>();
        }
    }
}
