using andywiecko.PBD2D.Core;
using UnityEngine;

namespace andywiecko.PBD2D.Components
{
    [RequireComponent(typeof(Ground))]
    [AddComponentMenu("PBD2D:Ground.Components/Collisions/Collide With TriMesh")]
    public class GroundCollideWithTriMesh : BaseComponent, IGroundCollideWithTriMesh
    {
        public Surface Surface => ground.Surface;

        private Ground ground;

        private void Start()
        {
            ground = GetComponent<Ground>();
        }
    }
}
