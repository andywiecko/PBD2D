using andywiecko.ECS;
using andywiecko.PBD2D.Core;
using System.Linq;
using Unity.Collections;
using UnityEngine;

namespace andywiecko.PBD2D.Components
{
    [Category(PBDCategory.Generator)]
    [RequireComponent(typeof(PointsLocker))]
    public class PointsLockerGeneratorAABB : PointsLockerGenerator
    {
        [SerializeField]
        private SpriteRenderer[] spriteRenderers = { };

        protected override void Start()
        {
            base.Start();
            DisposeOnDestroy(
                Constraints = new NativeList<PositionConstraint>(Positions.Value.Length, Allocator.Persistent)
            );

            var aabbs = spriteRenderers
                .Where(i => i != null)
                .Select(i => i.bounds.ToAABB())
                .ToArray();

            foreach (var (pId, p) in Positions.Value.IdsValues)
            {
                if (aabbs.Any(aabb => aabb.Contains(p)))
                {
                    Constraints.Value.AddNoResize(new(pId, p));
                }
            }

            OnConstraintsGeneration();
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.blue;

            var aabbs = spriteRenderers?
                .Where(i => i != null)
                .Select(i => i.bounds.ToAABB());

            foreach (var aabb in aabbs)
            {
                GizmosUtils.DrawAABB(aabb);
            }
        }
    }
}