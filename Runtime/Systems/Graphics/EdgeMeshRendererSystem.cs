using andywiecko.BurstCollections;
using andywiecko.ECS;
using andywiecko.PBD2D.Core;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace andywiecko.PBD2D.Systems
{
    public class EdgeMeshRendererSystem : BaseSystem<IEdgeMeshRenderer>
    {
        [BurstCompile]
        private struct CopyPositionsToMeshVerticesJob : IJob
        {
            private NativeIndexedArray<Id<Point>, float2>.ReadOnly positions;
            [ReadOnly]
            private NativeStackedLists<Id<Point>> segments;
            private NativeStackedLists<float3> vertices;

            public CopyPositionsToMeshVerticesJob(IEdgeMeshRenderer renderer)
            {
                positions = renderer.Positions.Value.AsReadOnly();
                segments = renderer.Segments;
                vertices = renderer.MeshVertices;
            }

            public void Execute()
            {
                for (int i = 0; i < segments.Length; i++)
                {
                    var s = segments[i];
                    var v = vertices[i];
                    for (int k = 0; k < s.Length; k++)
                    {
                        var id = s[k];
                        v[k] = new(positions[id], 0);
                    }
                }
            }
        }

        public override JobHandle Schedule(JobHandle dependencies)
        {
            foreach (var component in References)
            {
                dependencies = new CopyPositionsToMeshVerticesJob(component).Schedule(dependencies);
            }

            return dependencies;
        }

        [SolverAction]
        private void Redraw()
        {
            foreach (var component in References)
            {
                var renderers = component.Renderers;
                var mesh = component.MeshVertices.Value;
                for (int i = 0; i < renderers.Count; i++)
                {
                    renderers[i].SetPositions(mesh[i].Reinterpret<Vector3>());
                }
            }
        }
    }
}