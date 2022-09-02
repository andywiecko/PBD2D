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
        public Line Line => new(ground.Surface.Point, ground.Surface.Normal);
        public float Friction => ground.PhysicalMaterial.Friction;
        public float2 Displacement => pos - prevPos;

        [SerializeField]
        private Transform overrideDisplacementTransform;

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
            var t = overrideDisplacementTransform == null ? transform : overrideDisplacementTransform;
            pos = t.position.ToFloat2();
        }
    }
}