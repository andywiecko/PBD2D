using andywiecko.ECS;
using andywiecko.PBD2D.Core;
using Unity.Mathematics;
using UnityEngine;

namespace andywiecko.PBD2D.Components
{
    [RequireComponent(typeof(Ground))]
    [AddComponentMenu("PBD2D:Ground.Components/Collisions/Collide With TriMesh (Points)")]
    public class GroundLineCollideWithTriMeshPoints : BaseComponent, IGroundLineCollideWithTriMeshPoints
    {
        public Line Line => new(ground.Surface.Point, ground.Surface.Normal);
        public float Friction => ground.PhysicalMaterial.Friction;
        public float2 Displacement => pos - prevPos;

        private Ground ground;
        private float2 pos;
        private float2 prevPos;

        private void Start()
        {
            ground = GetComponent<Ground>();
        }

        private void Update()
        {
            prevPos = pos;
            pos = transform.position.ToFloat2();
        }
    }
}