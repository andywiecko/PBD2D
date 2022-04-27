using andywiecko.BurstCollections;
using andywiecko.BurstMathUtils;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace andywiecko.PBD2D.Core
{
    public interface ITriFieldLookup
    {
        Id<ExternalEdge> GetExternalEdge(Id<Triangle> triId, float3 barycoords);
    }

    public struct TriFieldLookup : ITriFieldLookup, INativeDisposable
    {
        private static readonly float2 a = math.float2(0, 0);
        private static readonly float2 b = math.float2(0, 1);
        private static readonly float2 c = math.float2(1, 1);
        private readonly float dx => 1f / (Samples - 1);

        public readonly int Samples;
        public bool IsInitialized;

        private NativeArray2d<Id<ExternalEdge>> mapping;
        private NativeArray<float3> barycoords;

        public TriFieldLookup(int trianglesCount, int samples, Allocator allocator)
        {
            Samples = samples;

            var barCount = samples * (samples + 1) / 2;
            barycoords = new(length: barCount, allocator);
            mapping = new(rowsCount: trianglesCount, colsCount: barCount, allocator);
            IsInitialized = false;
        }

        public JobHandle Initialize(JobHandle dependencies) => new InitializeBarycoordsJob(this).Schedule(dependencies);
        public void Initialize() => Initialize(default).Complete();
        public JobHandle GenerateMapping(
            NativeIndexedArray<Id<Point>, float2>.ReadOnly positions,
            NativeIndexedArray<Id<Triangle>, Triangle>.ReadOnly triangles,
            NativeIndexedArray<Id<ExternalEdge>, ExternalEdge>.ReadOnly externalEdges,
            JobHandle dependencies
        ) => new GenerateEdgeMappingsJob(this, positions, triangles, externalEdges).Schedule(dependencies);

        public ReadOnly AsReadOnly() => new(this);
        public Id<ExternalEdge> GetExternalEdge(Id<Triangle> triId, float3 bar) => AsReadOnly().GetExternalEdge(triId, bar);
        public JobHandle Dispose(JobHandle dependencies) => mapping.Dispose(barycoords.Dispose(dependencies));

        public void Dispose()
        {
            barycoords.Dispose();
            mapping.Dispose();
        }

        #region Jobs
        [BurstCompile]
        private struct InitializeBarycoordsJob : IJobParallelFor
        {
            private NativeArray<float3> barycoords;
            private readonly float dx;
            public InitializeBarycoordsJob(TriFieldLookup @this)
            {
                barycoords = @this.barycoords;
                dx = @this.dx;
            }

            public JobHandle Schedule(JobHandle dependencies)
            {
                return this.Schedule(barycoords.Length, innerloopBatchCount: 64, dependencies);
            }

            public void Execute(int index)
            {
                var (i, j) = MathUtils.ConvertFromTriMatId(index);
                var p = math.clamp(math.float2(dx * i, dx * j), 0, 1);
                barycoords[index] = MathUtils.Barycentric(a, b, c, p);
            }
        }

        [BurstCompile]
        private struct GenerateEdgeMappingsJob : IJobParallelFor
        {
            private NativeArray<float3>.ReadOnly barycoords;
            private NativeArray2d<Id<ExternalEdge>> mapping;
            private NativeIndexedArray<Id<Point>, float2>.ReadOnly positions;
            private NativeIndexedArray<Id<Triangle>, Triangle>.ReadOnly triangles;
            private NativeIndexedArray<Id<ExternalEdge>, ExternalEdge>.ReadOnly externalEdges;

            public GenerateEdgeMappingsJob(TriFieldLookup fieldLookup,
                NativeIndexedArray<Id<Point>, float2>.ReadOnly positions,
                NativeIndexedArray<Id<Triangle>, Triangle>.ReadOnly triangles,
                NativeIndexedArray<Id<ExternalEdge>, ExternalEdge>.ReadOnly externalEdges
                )
            {
                barycoords = fieldLookup.barycoords.AsReadOnly();
                mapping = fieldLookup.mapping;
                this.positions = positions;
                this.triangles = triangles;
                this.externalEdges = externalEdges;
            }

            public JobHandle Schedule(JobHandle dependencies)
            {
                return this.Schedule(mapping.RowsCount, innerloopBatchCount: 32, dependencies);
            }

            public void Execute(int index)
            {
                var row = mapping.GetRow(index);
                var triId = (Id<Triangle>)index;
                var (pA, pB, pC) = positions.At(triangles[triId]);
                for (int i = 0; i < barycoords.Length; i++)
                {
                    var bar = barycoords[i];
                    var p = bar.x * pA + bar.y * pB + bar.z * pC;
                    var edgeId = FindClosestEdge(p);
                    row[i] = edgeId;
                }
            }

            // Brute force, should be done smarter using kd tree or voronoi
            private Id<ExternalEdge> FindClosestEdge(float2 p)
            {
                var minDistanceSq = float.MaxValue;
                var closestEdge = Id<ExternalEdge>.Invalid;

                foreach (var (externalId, externalEdge) in externalEdges.IdsValues)
                {
                    var (a0, a1) = positions.At(externalEdge);
                    MathUtils.PointClosestPointOnLineSegment(p, a0, a1, out var q);
                    var distanceSq = math.distancesq(p, q);
                    if (distanceSq <= minDistanceSq)
                    {
                        closestEdge = externalId;
                        minDistanceSq = distanceSq;
                    }
                }
                return closestEdge;
            }
        }
        #endregion

        #region ReadOnly
        public struct ReadOnly : ITriFieldLookup
        {
            public NativeArray<float3>.ReadOnly Barycoords;
            private NativeArray2d<Id<ExternalEdge>>.ReadOnly mapping;
            private readonly float dx => 1f / (Samples - 1);
            public readonly int Samples;

            public ReadOnly(TriFieldLookup owner)
            {
                Barycoords = owner.barycoords.AsReadOnly();
                mapping = owner.mapping.AsReadOnly();
                Samples = owner.Samples;
            }

            public Id<ExternalEdge> GetExternalEdge(Id<Triangle> triId, float3 bar)
            {
                var p = bar.x * a + bar.y * b + bar.z * c;
                p /= dx;
                var (i, j) = (int2)math.round(p);
                var index = MathUtils.ConvertToTriMatId(i, j);
                return mapping[triId.Value, index];
            }
        }
        #endregion
    }
}