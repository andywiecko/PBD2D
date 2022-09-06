using andywiecko.ECS;
using andywiecko.PBD2D.Core;
using Unity.Mathematics;
using UnityEngine;

namespace andywiecko.PBD2D.Components
{
    [RequireComponent(typeof(Ground))]
    [Category(PBDCategory.Collisions)]
    public class GroundLineCollideWithTriMeshPoints : BaseComponent, IGroundLineCollideWithTriMeshPoints
    {
        public Line Line => ground.Line;
        public float Friction => ground.PhysicalMaterial.Friction;
        public float2 Displacement => ground.Displacement;

        private Ground ground;

        private void Start()
        {
            ground = GetComponent<Ground>();
        }
    }
}