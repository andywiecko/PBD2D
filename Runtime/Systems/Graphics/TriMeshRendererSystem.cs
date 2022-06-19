using andywiecko.BurstCollections;
using andywiecko.ECS;
using andywiecko.PBD2D.Core;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace andywiecko.PBD2D.Systems
{
    [Category(PBDCategory.Graphics)]
    public class TriMeshRendererSystem : BaseSystem<ITriMeshRenderer>
    {
        [BurstCompile]
        private struct CopyPositionsToMeshVerticesJob : IJobParallelFor
        {
            private NativeArray<float3> vertices;
            private NativeIndexedArray<Id<Point>, float2>.ReadOnly positions;

            public CopyPositionsToMeshVerticesJob(ITriMeshRenderer renderer)
            {
                vertices = renderer.MeshVertices.Value;
                positions = renderer.Positions.Value.AsReadOnly();
            }

            public JobHandle Schedule(JobHandle dependencies)
            {
                return this.Schedule(vertices.Length, 64, dependencies);
            }

            public void Execute(int index)
            {
                vertices[index] = positions[(Id<Point>)index].ToFloat3();
            }
        }

        [SolverAction]
        public void Redraw()
        {
            foreach (var renderer in References)
            {
                renderer.Redraw();
            }
        }

        public override JobHandle Schedule(JobHandle dependencies)
        {
            foreach (var renderer in References)
            {
                dependencies = new CopyPositionsToMeshVerticesJob(renderer).Schedule(dependencies);
            }

            return dependencies;
        }
    }
}
