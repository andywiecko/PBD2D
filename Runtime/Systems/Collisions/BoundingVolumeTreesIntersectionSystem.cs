using andywiecko.BurstCollections;
using andywiecko.PBD2D.Core;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace andywiecko.PBD2D.Systems
{
    [AddComponentMenu("PBD2D:Systems/Collisions/Bounding Volume Trees Intersection System")]
    public class BoundingVolumeTreesIntersectionSystem : BaseSystem<IBoundingVolumeTreesIntersectionTuple>
    {
        [BurstCompile]
        private struct GetTreesIntersectionJob : IJob
        {
            private NativeBoundingVolumeTree<AABB>.ReadOnly tree1;
            private NativeBoundingVolumeTree<AABB>.ReadOnly tree2;
            private NativeList<int2> result;

            public GetTreesIntersectionJob(IBoundingVolumeTreesIntersectionTuple tuple)
            {
                tree1 = tuple.Tree1.Value.AsReadOnly();
                tree2 = tuple.Tree2.Value.AsReadOnly();
                result = tuple.Result;
            }

            public void Execute() => tree1.GetIntersectionsWithTree(tree2, result);
        }

        public override JobHandle Schedule(JobHandle dependencies = default)
        {
            foreach (var component in References)
            {
                dependencies = new CommonJobs.ClearListJob<int2>(component.Result.Value).Schedule(dependencies);
                dependencies = new GetTreesIntersectionJob(component).Schedule(dependencies);
            }

            return dependencies;
        }
    }
}