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
            private readonly float4x4 w2l;
            private NativeArray<float3> vertices;
            private NativeIndexedArray<Id<Point>, float2>.ReadOnly positions;

            public CopyPositionsToMeshVerticesJob(ITriMeshRenderer renderer)
            {
                w2l = renderer.WorldToLocal;
                vertices = renderer.MeshVertices.Value;
                positions = renderer.Positions.Value.AsReadOnly();
            }

            public JobHandle Schedule(JobHandle dependencies) => this.Schedule(vertices.Length, 64, dependencies);
            public void Execute(int index) => vertices[index] = math.mul(w2l, math.float4(positions[(Id<Point>)index], 1, 1)).xyz;
        }

        [SolverAction]
        private void Redraw()
        {
            foreach (var renderer in References)
            {
                var mesh = renderer.Mesh;
                mesh.SetVertices(renderer.MeshVertices.Value);
                mesh.RecalculateBounds();
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
