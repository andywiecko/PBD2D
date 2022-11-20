using andywiecko.BurstCollections;
using andywiecko.ECS;
using andywiecko.PBD2D.Core;
using System;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace andywiecko.PBD2D.Components
{
    [DisallowMultipleComponent]
    public abstract class PointsLockerGenerator : BaseComponent
    {
        #region Inner types
        protected enum Type { Soft, Hard }

        protected class SoftLocks : IPositionConstraints
        {
            public Id<IComponent> ComponentId => owner.ComponentId;
            public float Stiffness => owner.Stiffness;
            public float Compliance => owner.Compliance;
            public Ref<NativeList<PositionConstraint>> Constraints => owner.Constraints;
            public Ref<NativeIndexedArray<Id<Point>, float2>> Positions => owner.Positions;
            public Ref<NativeIndexedArray<Id<Point>, float>> Weights => owner.Weights;
            private readonly PointsLockerGenerator owner;
            public SoftLocks(PointsLockerGenerator owner) => this.owner = owner;
        }

        protected class HardLocks : IPositionHardConstraints
        {
            public Id<IComponent> ComponentId => owner.ComponentId;
            public Ref<NativeList<PositionConstraint>> Constraints => owner.Constraints;
            public Ref<NativeIndexedArray<Id<Point>, float2>> Positions => owner.Positions;
            private readonly PointsLockerGenerator owner;
            public HardLocks(PointsLockerGenerator owner) => this.owner = owner;
        }
        #endregion

        public Ref<NativeIndexedArray<Id<Point>, float2>> Positions => locker.Positions;
        public Ref<NativeIndexedArray<Id<Point>, float>> Weights => locker.Weights;
        public Ref<NativeList<PositionConstraint>> Constraints { get; protected set; }

        [field: SerializeField, Range(0, 1), HideInInspector]
        public float Stiffness { get; private set; } = 1;

        [field: SerializeField, Min(0), HideInInspector]
        public float Compliance { get; private set; } = 0;

        [SerializeField, HideInInspector]
        protected Type type = Type.Soft;

        protected IComponent Locks => type switch
        {
            Type.Soft => softLocks ??= new(this),
            Type.Hard => hardLocks ??= new(this),
            _ => throw new NotImplementedException()
        };
        protected HardLocks hardLocks;
        protected SoftLocks softLocks;
        protected PointsLocker locker;

        protected override void OnEnable() => World.ComponentsRegistry.Add(Locks);
        protected override void OnDisable() => World.ComponentsRegistry.Remove(Locks);
        protected virtual void Start() => locker = GetComponent<PointsLocker>();

        protected void OnConstraintsGeneration()
        {
            switch (type)
            {
                case Type.Soft:
                    break;

                case Type.Hard:
                    DisableWeightsForConstraints();
                    break;
            }
        }

        private void DisableWeightsForConstraints()
        {
            foreach (var (pId, _) in Constraints.Value)
            {
                Weights.Value[pId] = 0;
            }
        }
    }
}