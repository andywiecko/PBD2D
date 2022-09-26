using andywiecko.BurstCollections;
using andywiecko.ECS;
using andywiecko.PBD2D.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Collections.NotBurstCompatible;
using Unity.Mathematics;
using UnityEngine;

namespace andywiecko.PBD2D.Components
{
    public class Rod : Entity, IPointsProvider
    {
        [field: SerializeField]
        public RodSerializedData SerializedData { get; private set; } = default;

        public IPhysicalMaterial PhysicalMaterial => physicalMaterial ? physicalMaterial : Core.PhysicalMaterial.Default;
        [SerializeField]
        private PhysicalMaterial physicalMaterial = default;

        public Ref<NativeArray<Point>> Points { get; private set; }
        public Ref<NativeIndexedArray<Id<Point>, float>> Weights { get; private set; }
        public Ref<NativeIndexedArray<Id<Point>, float2>> Positions { get; private set; }
        public Ref<NativeIndexedArray<Id<Point>, float2>> PreviousPositions { get; private set; }
        public Ref<NativeIndexedArray<Id<Point>, float2>> Velocities { get; private set; }
        public Ref<NativeIndexedArray<Id<Edge>, Edge>> Edges { get; private set; }
        public Ref<NativeMultiHashMap<Id<Point>, Id<Edge>>> PointToEdges { get; private set; }
        public Ref<NativeMultiHashMap<Id<Edge>, Id<Edge>>> EdgeNeighbors { get; private set; }
        public Ref<NativeMultiHashMap<int, Id<Edge>>> Segments { get; private set; }

        protected override void Awake()
        {
            base.Awake();

            if (!SerializedData)
            {
                foreach (var c in GetComponents<BaseComponent>())
                {
                    c.enabled = false;
                }
                throw new NullReferenceException();
            }

            var allocator = Allocator.Persistent;

            var positions = SerializedData.ToPositions(i => i);
            var pointsCount = positions.Length;

            DisposeOnDestroy(
                Points = new NativeArray<Point>(SerializedData.ToPoints(), allocator),
                Weights = new NativeIndexedArray<Id<Point>, float>(SerializedData.ToWeights(i => i, density: 1f), allocator),
                Positions = new NativeIndexedArray<Id<Point>, float2>(positions, allocator),
                PreviousPositions = new NativeIndexedArray<Id<Point>, float2>(positions, allocator),
                Velocities = new NativeIndexedArray<Id<Point>, float2>(pointsCount, allocator),
                Edges = new NativeIndexedArray<Id<Edge>, Edge>(SerializedData.ToEdges(), allocator),
                PointToEdges = new NativeMultiHashMap<Id<Point>, Id<Edge>>(pointsCount * pointsCount, allocator),
                EdgeNeighbors = new NativeMultiHashMap<Id<Edge>, Id<Edge>>(Edges.Value.Length * Edges.Value.Length, allocator)
            );

            var edges = Edges.Value.AsReadOnly();
            foreach (var (eId, (idA, idB)) in edges.IdsValues)
            {
                PointToEdges.Value.Add(idA, eId);
                PointToEdges.Value.Add(idB, eId);
            }

            var pointToEdges = PointToEdges.Value;
            foreach (var (eId, (idA, idB)) in edges.IdsValues)
            {
                AddNeighbors(idA);
                AddNeighbors(idB);

                void AddNeighbors(Id<Point> id)
                {
                    foreach (var other in PointToEdges.Value.GetValuesForKey(id))
                    {
                        if (other != eId)
                        {
                            EdgeNeighbors.Value.Add(eId, other);
                        }
                    }
                }
            }

            var points = Points.Value.AsReadOnly();
            using var visited = new NativeIndexedArray<Id<Edge>, bool>(edges.Length, Allocator.Temp);
            using var specialPoints = new NativeList<Point>(Points.Value.Length, Allocator.Temp);
            foreach (var p in points)
            {
                var values = pointToEdges.GetValuesForKey(p.Id);
                var enumerables = AsEnumerable();
                if (enumerables.Count() != 2)
                {
                    specialPoints.Add(p);
                }

                IEnumerable<Id<Edge>> AsEnumerable()
                {
                    foreach (var v in values)
                    {
                        yield return v;
                    }
                }
            }

            foreach (var p in specialPoints)
            {
                Debug.Log(p.Id);
            }
            // 1. grab points to edges count different than 2
            // 2. dequeue until no point will remain
        }

        private void OnDrawGizmos()
        {
            if (!SerializedData)
            {
                return;
            }

            // TODO: add transformation

            ReadOnlySpan<float2> positions = Application.isPlaying ? Positions.Value : SerializedData.Positions;
            foreach (var p in positions)
            {
                GizmosUtils.DrawCircle(p, r: 0.1f);
            }

            IEnumerable<(int, int)> edges = Application.isPlaying ?
                Edges.Value.Select(i => ((int)i.IdA, (int)i.IdB)) :
                Enumerable
                    .Range(0, SerializedData.Edges.Length / 2)
                    .Select(i =>
            {
                var e = SerializedData.Edges;
                return (e[2 * i], e[2 * i + 1]);
            });

            foreach (var (eA, eB) in edges)
            {
                GizmosUtils.DrawLine(positions[eA], positions[eB]);
            }
        }
    }
}