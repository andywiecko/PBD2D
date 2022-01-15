using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace andywiecko.PBD2D.Core
{
    public static class CommonJobs 
    {
        [BurstCompile]
        public struct SetNativeReferenceValueJob<T> : IJob
            where T : unmanaged
        {
            private NativeReference<T> reference;
            private readonly T value;

            public SetNativeReferenceValueJob(NativeReference<T> reference, T value)
            {
                this.reference = reference;
                this.value = value;
            }

            public void Execute()
            {
                reference.Value = value;
            }
        }

        [BurstCompile]
        public struct ClearHashSetJob<T> : IJob where T : unmanaged, IEquatable<T>
        {
            private NativeHashSet<T> hashSet;
            public ClearHashSetJob(NativeHashSet<T> hashSet) => this.hashSet = hashSet;
            public void Execute() => hashSet.Clear();
        }

        [BurstCompile]
        public struct ClearListJob<T> : IJob where T : unmanaged
        {
            private NativeList<T> list;
            public ClearListJob(NativeList<T> list) => this.list = list;
            public void Execute() => list.Clear();
        }
    }
}