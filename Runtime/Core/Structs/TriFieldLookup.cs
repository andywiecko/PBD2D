using andywiecko.BurstCollections;
using andywiecko.BurstMathUtils;
using System.Diagnostics;
using System;
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

        public readonly int Samples;

        public NativeArray<float3> Barycoords;
        public NativeReference<bool> IsInitialized;
        private NativeArray2d<Id<ExternalEdge>> mapping;

        public TriFieldLookup(int trianglesCount, int samples, Allocator allocator)
        {
            Samples = samples;

            var barCount = samples * (samples + 1) / 2;
            Barycoords = new(length: barCount, allocator);
            mapping = new(rowsCount: trianglesCount, colsCount: barCount, allocator);
            IsInitialized = new(allocator);
        }

        public JobHandle Initialize(
            NativeIndexedArray<Id<Point>, float2>.ReadOnly positions,
            NativeIndexedArray<Id<Triangle>, Triangle>.ReadOnly triangles,
            NativeIndexedArray<Id<ExternalEdge>, ExternalEdge>.ReadOnly externalEdges,
            JobHandle dependencies = default)
        {
            dependencies = Samples == 1 ?
                new CommonJobs.SetNativeArrayValueJob<float3>(Barycoords, 1f / 3).Schedule(dependencies) :
                new InitializeBarycoordsJob(this).Schedule(dependencies);
            dependencies = new GenerateEdgeMappingsJob(this, positions, triangles, externalEdges).Schedule(dependencies);
            dependencies = new CommonJobs.SetNativeReferenceValueJob<bool>(IsInitialized, true).Schedule(dependencies);

            return dependencies;
        }

        public ReadOnly AsReadOnly() => new(this);
        public Id<ExternalEdge> GetExternalEdge(Id<Triangle> triId, float3 bar) => AsReadOnly().GetExternalEdge(triId, bar);
        public JobHandle Dispose(JobHandle dependencies) => IsInitialized.Dispose(mapping.Dispose(Barycoords.Dispose(dependencies)));

        public void Dispose()
        {
            IsInitialized.Dispose();
            Barycoords.Dispose();
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
                barycoords = @this.Barycoords;
                dx = 1f / (@this.Samples - 1);
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
                barycoords = fieldLookup.Barycoords.AsReadOnly();
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

            // TODO: Brute force, should be done smarter using kd tree or voronoi
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
            private NativeReference<bool>.ReadOnly IsInitialized;
            private NativeArray2d<Id<ExternalEdge>>.ReadOnly mapping;
            private readonly float dx;

            public ReadOnly(TriFieldLookup owner)
            {
                IsInitialized = owner.IsInitialized.AsReadOnly();
                mapping = owner.mapping.AsReadOnly();
                dx = 1f / (owner.Samples - 1);
            }

            public Id<ExternalEdge> GetExternalEdge(Id<Triangle> triId, float3 bar)
            {
                CheckIfTreeIsInitialized(IsInitialized.Value);
                CheckBarycentricRange(bar);

                bar = math.clamp(bar, 0, 1);
                var p = bar.x * a + bar.y * b + bar.z * c;
                var (i, j) = (int2)math.round(p / dx);
                var index = MathUtils.ConvertToTriMatId(i, j);
                return mapping[triId.Value, index];
            }
        }
        #endregion

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        private static void CheckIfTreeIsInitialized(bool isInitialized)
        {
            if (!isInitialized)
            {
                throw new Exception(
                    $"{nameof(TriFieldLookup)} has not been initialized! " +
                    $"One should initialize field before using it."
                );
            }
        }

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        private static void CheckBarycentricRange(float3 b)
        {
            if (math.any(b < 0 | b > 1))
            {
                throw new ArgumentOutOfRangeException(
                    $"[{nameof(TriFieldLookup)}]: Input barycentric b = {b} coordinate should be in bᵢ∈[0, 1]."
                );
            }
        }
    }
}