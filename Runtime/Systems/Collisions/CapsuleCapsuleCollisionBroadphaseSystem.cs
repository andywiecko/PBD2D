using andywiecko.BurstCollections;
using andywiecko.PBD2D.Core;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace andywiecko.PBD2D.Systems
{
    [AddComponentMenu("PBD2D:Systems/Collisions/Capsule Capsule Collision System (Broadphase)")]
    public class CapsuleCapsuleCollisionBroadphaseSystem : BaseSystem<ICapsuleCapsuleCollisionBroadphaseTuple>
    {
        [BurstCompile]
        private struct CheckForPotentialCollisionsTree : IJob
        {
            private NativeBoundingVolumeTree<AABB>.ReadOnly tree1;
            private NativeBoundingVolumeTree<AABB>.ReadOnly tree2;
            private NativeList<IdPair<CollidableEdge>> potentialCollisions;

            public CheckForPotentialCollisionsTree(ICapsuleCapsuleCollisionBroadphaseTuple tuple)
            {
                tree1 = tuple.Component1.Tree.Value.AsReadOnly();
                tree2 = tuple.Component2.Tree.Value.AsReadOnly();
                potentialCollisions = tuple.PotentialCollisions;
            }

            public void Execute()
            {
                var result = UnsafeUtility.As<NativeList<IdPair<CollidableEdge>>, NativeList<int2>>(ref potentialCollisions);
                tree1.GetIntersectionsWithTree(tree2, result);
            }
        }

        public override JobHandle Schedule(JobHandle dependencies = default)
        {
            foreach (var component in References)
            {
                dependencies = new CommonJobs.ClearListJob<IdPair<CollidableEdge>>(component.PotentialCollisions.Value).Schedule(dependencies);
                dependencies = new CheckForPotentialCollisionsTree(component).Schedule(dependencies);
            }

            return dependencies;
        }

#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
        private static void CheckStructLayouts()
        {
            if (UnsafeUtility.SizeOf<IdPair<CollidableEdge>>() != UnsafeUtility.SizeOf<int2>())
            {
                Debug.LogError(
                    $"[{nameof(CapsuleCapsuleCollisionBroadphaseSystem)}]: " +
                    $"{nameof(IdPair<CollidableEdge>)} has different layout than {nameof(int2)}. " +
                    "Buffer after reinterpretion will contain garbage data!"
                );
            }
        }
#endif
    }
}