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

        private NativeArray<Id<ExternalEdge>> externalEdges;
        private NativeArray<float3> barycoords;

        public TriFieldLookup(int trianglesCount, int samples, Allocator allocator)
        {
            Samples = samples;

            var barCount = samples * (samples + 1) / 2;
            barycoords = new(length: barCount, allocator);
            externalEdges = new(barCount * trianglesCount, allocator);
            IsInitialized = false;
        }

        public JobHandle Initialize(JobHandle dependencies) => new InitializeJob(this).Schedule(dependencies);
        public void Initialize() => Initialize(default).Complete();
        public ReadOnly AsReadOnly() => new(this);

        public Id<ExternalEdge> GetExternalEdge(Id<Triangle> triId, float3 bar)
        {
            var p = bar.x * a + bar.y * b + bar.z * c;
            p /= dx;
            var (i, j) = (int2)math.round(p);
            var index = MathUtils.ConvertToTriMatId(i, j);


            throw new System.NotImplementedException();
        }

        public JobHandle Dispose(JobHandle dependencies) => externalEdges.Dispose(barycoords.Dispose(dependencies));
        
        public void Dispose()
        {
            barycoords.Dispose();
            externalEdges.Dispose();
        }

        [BurstCompile]
        private struct InitializeJob : IJobParallelFor
        {
            private NativeArray<float3> barycoords;
            private readonly float dx;
            public InitializeJob(TriFieldLookup @this)
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

        public struct ReadOnly : ITriFieldLookup
        {
            private NativeArray<float3>.ReadOnly barycoords;
            private NativeArray<Id<ExternalEdge>>.ReadOnly externalEdges;

            public ReadOnly(TriFieldLookup owner)
            {
                barycoords = owner.barycoords.AsReadOnly();
                externalEdges = owner.externalEdges.AsReadOnly();
            }

            public Id<ExternalEdge> GetExternalEdge(Id<Triangle> triId, float3 barycoords)
            {
                throw new System.NotImplementedException();
            }
        }
    }
}