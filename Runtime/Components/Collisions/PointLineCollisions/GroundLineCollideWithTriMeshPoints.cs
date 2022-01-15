using andywiecko.PBD2D.Core;
using UnityEngine;

namespace andywiecko.PBD2D.Components
{
    [RequireComponent(typeof(Ground))]
    [AddComponentMenu("PBD2D:Ground.Components/Collisions/Collide With TriMesh (Points)")]
    public class GroundLineCollideWithTriMeshPoints : BaseComponent, IGroundLineCollideWithTriMeshPoints
    {
        public Line Line => new (ground.Surface.Point, ground.Surface.Normal);

        private Ground ground;

        private void Start()
        {
            ground = GetComponent<Ground>();
        }
    }
}